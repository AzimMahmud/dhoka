using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Comments.Get;
using Domain.Comments;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Posts.Comment;

internal sealed class GetCommentQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetCommentQuery, List<CommentsResponse>>
{
    public async Task<Result<List<CommentsResponse>>> Handle(GetCommentQuery query, CancellationToken cancellationToken)
    {
        List<CommentsResponse> comments = await context.Comments
            .Where(comment => comment.PostId == query.PostId)
            .Select(post => new CommentsResponse(
                post.Id,
                post.PostId,
                post.ContactInfo,
                post.Description
            ))
            .ToListAsync(cancellationToken);


        return !comments.Any() ? [] : comments;
    }
}
