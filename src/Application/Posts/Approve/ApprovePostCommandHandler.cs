using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Posts.Approve;

internal sealed class ApprovePostCommandHandler(
    IApplicationDbContext context)
    : ICommandHandler<ApprovePostCommand>
{
    public async Task<Result> Handle(ApprovePostCommand command, CancellationToken cancellationToken)
    {
        Post? post = await context.Posts
            .SingleOrDefaultAsync(t => t.Id == command.PostId, cancellationToken);

        if (post is null)
        {
            return Result.Failure(PostErrors.NotFound(command.PostId));
        }

        if (post.IsApproved)
        {
            return Result.Failure(PostErrors.AlreadyApproved(command.PostId));
        }
        

        post.Status = nameof(Status.Approved);
        
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
