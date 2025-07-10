using Application.Abstractions.Messaging;
using Domain.Posts;
using SharedKernel;
using Status = Domain.Posts.Status;

namespace Application.Posts.Settled;

internal sealed class SettledPostCommandHandler(
    IPostRepository postRepository, IPostCounterRepository postCounterRepository)
    : ICommandHandler<SettledPostCommand>
{
    public async Task<Result> Handle(SettledPostCommand command, CancellationToken cancellationToken)
    {
        Post? post =  await postRepository.GetByIdAsync(command.PostId);
        
        if (post.Id == Guid.Empty)
        {
            return Result.Failure(PostErrors.NotFound(command.PostId));
        }
        
        if (!post.IsApproved)
        {
            return Result.Failure(PostErrors.NotApproved(command.PostId));
        }

        post.Status = nameof(Status.Settled);
        
        await postRepository.UpdateAsync(post);
        
        await postCounterRepository.IncrementAsync(nameof(Status.Settled), 1);

        return Result.Success();
    }
}
