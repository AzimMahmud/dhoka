using Application.Abstractions.Messaging;
using Domain.Posts;
using SharedKernel;

namespace Application.Posts.Delete;

internal sealed class DeletePostCommandHandler(IPostRepository postRepository, IPostCounterRepository postCounterRepository, IImageService imageService)
    : ICommandHandler<DeletePostCommand>
{
    public async Task<Result> Handle(DeletePostCommand command, CancellationToken cancellationToken)
    {
        Post? post =  await postRepository.GetByIdAsync(command.PostId);
        int totalPost = await postCounterRepository.GetCountAsync("Posts");
        
        
        if (post is null)
        {
            return Result.Failure(PostErrors.NotFound(command.PostId));
        }

        if (post.IsApproved)
        {
            return Result.Failure(PostErrors.AlreadyApproved(command.PostId));
        }

        if (post.ImageUrls != null && post.ImageUrls.Any())
        {
            await imageService.DeleteImageFromCloudFrontUrlAsync(post.ImageUrls);
        }

        await postRepository.DeleteAsync(command.PostId);

        if (totalPost > 0)
        {
            await postCounterRepository.SetCountAsync("Posts", totalPost - 1);
        }
        
        return Result.Success();
    }
}
