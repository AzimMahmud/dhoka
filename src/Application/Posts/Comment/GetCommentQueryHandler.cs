using Application.Abstractions.Messaging;
using Domain.Comments;
using SharedKernel;

namespace Application.Posts.Comment;

internal sealed class GetCommentQueryHandler(ICommentRepository commentRepository)
    : IQueryHandler<GetCommentQuery, List<CommentsResponse>>
{
    public async Task<Result<List<CommentsResponse>>> Handle(GetCommentQuery query, CancellationToken cancellationToken)
    {
        List<CommentsResponse> comments = await commentRepository.GetByPostIdAsync(query.PostId);
        
        return !comments.Any() ? [] : comments;
    }
}
