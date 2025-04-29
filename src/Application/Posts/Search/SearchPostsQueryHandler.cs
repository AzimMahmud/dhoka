using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.SearchEvents;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

// for NpgsqlTsVector


namespace Application.Posts.Search;

internal sealed class SearchPostsQueryHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    : IQueryHandler<SearchPostsQuery, SearchPagedList>
{
    public async Task<Result<SearchPagedList>> Handle(SearchPostsQuery request,
        CancellationToken cancellationToken)
    {
        int pageNumber = request.Page;
        int pageSize = request.PageSize;
        string searchTerm = request.SearchTerm;

        if ( string.IsNullOrEmpty(searchTerm))
        {
            return new Result<SearchPagedList>(new SearchPagedList(), true, Error.None);
        }
        
        var searchEvent = new SearchEvent
        {
            Query = request.SearchTerm,
            Timestamp = dateTimeProvider.UtcNow
        };
        
        await context.SearchEvents.AddAsync(searchEvent, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        
        int totalCount = await context.Posts
            .FromSqlInterpolated($@"
                SELECT Id
                FROM posts
                WHERE search_vector @@plainto_tsquery('simple', {searchTerm})")
            .CountAsync(cancellationToken: cancellationToken);


        List<SearchPostsResponse> postsQuery = await context.Posts
            .FromSqlInterpolated($@"
                SELECT id, title, transaction_mode, payment_type, description, mobil_numbers, amount, created_at
                FROM posts
                WHERE search_vector @@plainto_tsquery('simple', {searchTerm})
                ORDER BY ts_rank(search_vector, plainto_tsquery('simple', {searchTerm})) DESC
                LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}")
            .Select(p => new SearchPostsResponse
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
            .AsNoTracking()
            .ToListAsync(cancellationToken: cancellationToken);

        var posts = SearchPagedList.CreateAsync(
            postsQuery,
            request.Page,
            request.PageSize,
            totalCount);


        return posts;
    }
}
