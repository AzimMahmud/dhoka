using Application.Abstractions.Messaging;
using Domain.Posts;
using Microsoft.Extensions.Caching.Memory;
using SharedKernel;
using Humanizer;
using Status = Domain.Posts.Status;

namespace Application.Metrics.SearchMetrics;

public sealed record SearchMetricsQuery : IQuery<SearchMetricDto>;

internal class SearchMetricsQueryHandler(IPostCounterRepository repository, IMemoryCache cache)
    : IQueryHandler<SearchMetricsQuery, SearchMetricDto>
{
    public async Task<Result<SearchMetricDto>> Handle(SearchMetricsQuery request, CancellationToken cancellationToken)
    {
        // 1. Kick off all count queries in parallel:
        Task<int> totalSearchesTask      = TotalSearches();
        Task<int> totalPostsTask         = TotalPosts();
        Task<int> totalApprovedPostsTask = TotalApprovedPosts();
        Task<int> totalSettledPostsTask  = TotalSettledPosts();

        await Task.WhenAll(
            totalSearchesTask,
            totalPostsTask,
            totalApprovedPostsTask,
            totalSettledPostsTask
        );

        // 2. Extract raw integers
        int totalSearches       = await totalSearchesTask;
        int totalPosts          = await totalPostsTask;
        int totalApprovedPosts  = await totalApprovedPostsTask;
        int totalSettledPosts   = await totalSettledPostsTask;

        // 3. Build DTO with metric‐formatted strings
        var dto = new SearchMetricDto
        {
            TotalSearches      = totalSearches.ToMetric(),
            TotalPosts         = totalPosts.ToMetric(),
            TotalApprovedPosts = totalApprovedPosts.ToMetric(),
            TotalSettledPosts  = totalSettledPosts.ToMetric(),
        };

        // 4. Wrap in Result<T>. 
        return Result.Success(dto);
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
