using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Comments;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Comments.GetById;

internal sealed class GetCommentByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetCommentByIdQuery, CommentResponse>
{
    public async Task<Result<CommentResponse>> Handle(GetCommentByIdQuery query, CancellationToken cancellationToken)
    {
        CommentResponse? comment = await context.Comments
            .Where(comment => comment.Id == query.CommentId )
            .Select(post => new CommentResponse(
                    post.Id,
                    post.PostId,
                    post.ContactInfo,
                    post.Description,
                    post.CreatedAt
                ))
                .SingleOrDefaultAsync(cancellationToken);

        if (comment is null)
        {
            return Result.Failure<CommentResponse>(CommentErrors.NotFound(query.CommentId));
        }

        return comment;
    }
}
