using Application.Abstractions.Messaging;
using Domain;
using Domain.Posts;
using SharedKernel;

namespace Application.Posts.Search;

internal sealed class SearchPostsQueryHandler(IPostRepository postRepository, IPostCounterRepository postCounterRepository)
    : IQueryHandler<SearchPostsQuery, PagedSearchResult<PostsResponse>>
{
    public async Task<Result<PagedSearchResult<PostsResponse>>> Handle(SearchPostsQuery request,
        CancellationToken cancellationToken)
    {
        PagedSearchResult<PostsResponse> posts = await postRepository.SearchAsync(request.SearchRequest);
        
        await postCounterRepository.IncrementAsync("Search", 1);

        return posts;
    }
}
