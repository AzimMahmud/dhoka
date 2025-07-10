using Application.Abstractions.Messaging;
using Domain.Posts;
using SharedKernel;
using Status = Domain.Posts.Status;

namespace Application.Posts.Admin.ReIndex;

internal sealed class ReIndexPostCommandHandler(
    IPostRepository postRepository,
    IPostCounterRepository postCounterRepository)
    : ICommandHandler<ReIndexPostCommand>
{
    public async Task<Result> Handle(ReIndexPostCommand command, CancellationToken cancellationToken)
    {
        Post? post = await postRepository.GetByIdAsync(command.PostId);

        if (post is null)
        {
            return Result.Failure(PostErrors.NotFound(command.PostId));
        }

        if (!post.IsApproved)
        {
            return Result.Failure(PostErrors.NotApproved(command.PostId));
        }

        await postRepository.EnsurePostIndexedAsync(post.Id);

        return Result.Success();
    }
}
