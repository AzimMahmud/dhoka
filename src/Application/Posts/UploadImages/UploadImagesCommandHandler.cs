using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Posts.UploadImages;

internal sealed class UploadImagesCommandHandler(IPostRepository postRepository, IImageService imageService)
    : ICommandHandler<UploadImagesCommand>
{
    public async Task<Result> Handle(UploadImagesCommand command, CancellationToken cancellationToken)
    {
        // Post? post = await context.Posts.AsNoTracking()
        //     .FirstOrDefaultAsync(x => x.Id == command.PostId, cancellationToken: cancellationToken);

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
        
        // context.Posts.Update(post);
        //
        // await context.SaveChangesAsync(cancellationToken);

        return Result.Success("Image uploaded");
    }
}
