using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Posts.Settled;

internal sealed class SettledPostCommandHandler(
    IApplicationDbContext context)
    : ICommandHandler<SettledPostCommand>
{
    public async Task<Result> Handle(SettledPostCommand command, CancellationToken cancellationToken)
    {
        Post? post = await context.Posts
            .SingleOrDefaultAsync(t => t.Id == command.PostId, cancellationToken);

        if (post is null)
        {
            return Result.Failure(PostErrors.NotFound(command.PostId));
        }
        
        if (!post.IsApproved)
        {
            return Result.Failure(PostErrors.NotApproved(command.PostId));
        }
        
        post.IsSettled = true;
        
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
