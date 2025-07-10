using Application.Abstractions.Messaging;
using Domain.Comments;
using SharedKernel;

namespace Application.Comments.GetById;

internal sealed class GetCommentByIdQueryHandler(ICommentRepository commentRepository)
    : IQueryHandler<GetCommentByIdQuery, CommentResponse>
{
    public async Task<Result<CommentResponse>> Handle(GetCommentByIdQuery query, CancellationToken cancellationToken)
    {
        CommentResponse? comment = await commentRepository.GetByIdAsync(query.CommentId);

        return comment ?? Result.Failure<CommentResponse>(CommentErrors.NotFound(query.CommentId));
    }
}
