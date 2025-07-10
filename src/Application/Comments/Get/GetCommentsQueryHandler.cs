using Application.Abstractions.Messaging;
using Domain;
using Domain.Comments;
using SharedKernel;

namespace Application.Comments.Get;

internal sealed class GetCommentsQueryHandler(ICommentRepository commentRepository)
    : IQueryHandler<GetCommentsQuery, PagedResult<CommentsResponse>>
{
    public async Task<Result<PagedResult<CommentsResponse>>> Handle(GetCommentsQuery request,
        CancellationToken cancellationToken)
    {
        PagedResult<CommentsResponse> comments =
            await commentRepository.GetByPostIdPaginatedAsync(request.PostId, request.PageSize, request.PaginationToken);

        return comments;
    }
}
