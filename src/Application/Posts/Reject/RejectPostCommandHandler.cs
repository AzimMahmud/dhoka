using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Posts.Reject;

internal sealed class RejectPostCommandHandler(
    IPostRepository postRepository)
    : ICommandHandler<RejectPostCommand>
{
    public async Task<Result> Handle(RejectPostCommand command, CancellationToken cancellationToken)
    {
        // Post? post = await context.Posts
        //     .SingleOrDefaultAsync(t => t.Id == command.PostId, cancellationToken);
        
        Post? post =  await postRepository.GetByIdAsync(command.PostId);

        if (post is null)
        {
            return Result.Failure(PostErrors.NotFound(command.PostId));
        }

        if (post.IsApproved)
        {
            return Result.Failure(PostErrors.AlreadyApproved(command.PostId));
        }
        
        post.Status = nameof(Status.Rejected);

        await postRepository.UpdateAsync(post);
        
        // await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
