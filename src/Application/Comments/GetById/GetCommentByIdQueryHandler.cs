using Application.Abstractions.Messaging;
using Domain.Comments;
using SharedKernel;

namespace Application.Comments.GetById;

internal sealed class GetCommentByIdQueryHandler(ICommentRepository commentRepository)
    : IQueryHandler<GetCommentByIdQuery, CommentResponse>
{
    public async Task<Result<CommentResponse>> Handle(GetCommentByIdQuery query, CancellationToken cancellationToken)
    {
        // CommentResponse? comment = await context.Comments
        //     .Where(comment => comment.Id == query.CommentId )
        //     .Select(post => new CommentResponse(
        //             post.Id,
        //             post.PostId,
        //             post.ContactInfo,
        //             post.Description,
        //             post.CreatedAt
        //         ))
        //         .SingleOrDefaultAsync(cancellationToken);


        CommentResponse? comment = await commentRepository.GetByIdAsync(query.CommentId);

        if (comment is null)
        {
            return Result.Failure<CommentResponse>(CommentErrors.NotFound(query.CommentId));
        }

        return comment;
    }
}
