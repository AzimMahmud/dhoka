using Application.Abstractions.Messaging;
using Domain;
using Domain.Posts;
using Microsoft.Extensions.Caching.Memory;
using SharedKernel;

namespace Application.Posts.RecentPosts;

public sealed record RecentPostsQuery : IQuery<List<PostsResponse>>;

internal class RecentPostsQueryHandler(
    IPostRepository postRepository,
    IMemoryCache cache)
    : IQueryHandler<RecentPostsQuery, List<PostsResponse>>
{
    public async Task<Result<List<PostsResponse>>> Handle(RecentPostsQuery request, CancellationToken cancellationToken)
    {
        List<PostsResponse>? recentPosts = await cache.GetOrCreateAsync("RecentPosts", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await postRepository.GetRecentPostsAsync();
        });

        return recentPosts;
    }
}
