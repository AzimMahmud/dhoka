using Application.Abstractions.Messaging;
using Domain.Posts;
using SharedKernel;

namespace Application.Posts.Admin.Reject;

internal sealed class RejectPostCommandHandler(
    IPostRepository postRepository)
    : ICommandHandler<RejectPostCommand>
{
    public async Task<Result> Handle(RejectPostCommand command, CancellationToken cancellationToken)
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
        
        post.Status = nameof(Domain.Posts.Status.Rejected);

        await postRepository.UpdateAsync(post);

        return Result.Success();
    }
}
