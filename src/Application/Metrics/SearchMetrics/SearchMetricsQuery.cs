using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SharedKernel;

namespace Application.Metrics.SearchMetrics;

public sealed record SearchMetricsQuery : IQuery<SearchMetricDto>;

internal class SearchMetricsQueryHandler(IApplicationDbContext db, IMemoryCache cache)
    : IQueryHandler<SearchMetricsQuery, SearchMetricDto>
{
    public async Task<Result<SearchMetricDto>> Handle(SearchMetricsQuery request, CancellationToken cancellationToken)
    {
        return new SearchMetricDto
        {
            TotalSearches = await TotalSearches(),
            TotalPosts = await TotalPosts(),
            TotalApprovedPosts = await TotalApprovedPosts(),
            TotalSettledPosts = await TotalSettledPosts(),
        };
    }
    
    
    
    private async Task<int> GetMetricAsync(string key, Func<Task<int>> fetch)
    {
        return await cache.GetOrCreateAsync(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return new Lazy<Task<int>>(fetch).Value;
        });
    }

    public Task<int> TotalSearches() =>
        GetMetricAsync("TotalSearches", () => db.SearchEvents.CountAsync());

    public Task<int> TotalPosts() =>
        GetMetricAsync("TotalPosts", () => db.Posts.Where(p => p.Status != nameof(Domain.Posts.Status.Init)).CountAsync());

    public Task<int> TotalApprovedPosts() =>
        GetMetricAsync("TotalApprovedPosts", () => 
            db.Posts.Where(p => p.Status == nameof(Domain.Posts.Status.Approved)).CountAsync());
    
    public Task<int> TotalSettledPosts() =>
        GetMetricAsync("TotalSettledPosts", () => 
            db.Posts.Where(p => p.IsSettled).CountAsync());
}
