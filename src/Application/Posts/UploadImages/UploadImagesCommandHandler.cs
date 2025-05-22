using Application.Abstractions.Messaging;
using Domain.Posts;
using SharedKernel;

namespace Application.Posts.UploadImages;

internal sealed class UploadImagesCommandHandler(IPostRepository postRepository, IImageService imageService)
    : ICommandHandler<UploadImagesCommand>
{
    public async Task<Result> Handle(UploadImagesCommand command, CancellationToken cancellationToken)
    {
        Post? post =  await postRepository.GetByIdAsync(command.PostId);
        
        if (post is null)
        {
            return Result.Failure(PostErrors.NotFound(command.PostId));
        }

        List<string> urls = await imageService.UploadImagesAsync(command.Images, command.PostId);

        if (urls.Any())
        {
            post.ImageUrls = urls;
        
            await postRepository.UpdateAsync(post);
        }

        return Result.Success("Image uploaded");
    }
}
