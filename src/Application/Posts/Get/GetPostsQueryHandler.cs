using Application.Abstractions.Messaging;
using Domain;
using Domain.Posts;
using SharedKernel;

namespace Application.Posts.Get;

internal sealed class GetPostsQueryHandler(IPostRepository postRepository)
    : IQueryHandler<GetPostsQuery, PagedResult<PostsResponse>>
{
    public async Task<Result<PagedResult<PostsResponse>>> Handle(GetPostsQuery request,
        CancellationToken cancellationToken)
    {
        PagedResult<PostsResponse> posts = await postRepository.GetAllAsync(request.PageSize, request.PaginationToken, request.status);

        return posts;
    }
}
