using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Posts.Search;
using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SharedKernel;

namespace Application.Posts.RecentPosts;

public sealed record RecentPostsQuery : IQuery<List<RecentPostDto>>;

internal class RecentPostsQueryHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    IMemoryCache cache)
    : IQueryHandler<RecentPostsQuery, List<RecentPostDto>>
{
    public async Task<Result<List<RecentPostDto>>> Handle(RecentPostsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<RecentPostDto> recentPostsQuery = context.Posts
            .FromSqlInterpolated(
                $"SELECT id, title, transaction_mode, payment_type, description, mobil_numbers, amount, created_at FROM posts WHERE status = {nameof(Status.Approved)} order by created_at LIMIT 5")
            .Select(p => new RecentPostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    TransactionMode = p.TransactionMode,
                    PaymentType = p.PaymentType,
                    Description = p.Description,
                    MobilNumbers = p.MobilNumbers,
                    Amount = p.Amount,
                    CreatedAt = p.CreatedAt,
                    Created = dateTimeProvider.ToRelativeTime(p.CreatedAt)
                }
            )
            .AsQueryable();

        List<RecentPostDto>? recentPosts = await cache.GetOrCreateAsync("RecentPosts", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await recentPostsQuery.ToListAsync(cancellationToken: cancellationToken);
        });

        return recentPosts;
    }
}
