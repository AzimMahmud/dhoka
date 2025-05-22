using Application.Abstractions.Messaging;
using Domain.Posts;
using SharedKernel;

namespace Application.Posts.Approve;

internal sealed class ApprovePostCommandHandler(
    IPostRepository postRepository, IPostCounterRepository postCounterRepository)
    : ICommandHandler<ApprovePostCommand>
{
    public async Task<Result> Handle(ApprovePostCommand command, CancellationToken cancellationToken)
    {
        Post? post =  await postRepository.GetByIdAsync(command.PostId);
        
        if (post is null)
        {
            return Result.Failure(PostErrors.NotFound(command.PostId));
        }

        if (post.IsApproved)
        {
            return Result.Failure(PostErrors.AlreadyApproved(command.PostId));
        }

        post.Status = nameof(Status.Approved);
        
       await postRepository.UpdateAsync(post);
       await postCounterRepository.IncrementAsync(nameof(Status.Approved), 1);

        return Result.Success();
    }
}
