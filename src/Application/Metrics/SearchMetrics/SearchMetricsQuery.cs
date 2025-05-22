using Application.Abstractions.Messaging;
using Domain.Posts;
using Microsoft.Extensions.Caching.Memory;
using SharedKernel;

namespace Application.Metrics.SearchMetrics;

public sealed record SearchMetricsQuery : IQuery<SearchMetricDto>;

internal class SearchMetricsQueryHandler(IPostCounterRepository repository, IMemoryCache cache)
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
        GetMetricAsync("TotalSearches", () => repository.GetCountAsync("Search"));

    public Task<int> TotalPosts() =>
        GetMetricAsync("TotalPosts", () => repository.GetCountAsync("Posts"));

    public Task<int> TotalApprovedPosts() =>
        GetMetricAsync("TotalApprovedPosts", () => repository.GetCountAsync(nameof(Status.Approved)));

    public Task<int> TotalSettledPosts() =>
        GetMetricAsync("TotalSettledPosts", () =>
            repository.GetCountAsync(nameof(Status.Settled)));
}
