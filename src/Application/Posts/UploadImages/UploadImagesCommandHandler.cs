using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Posts.UploadImages;

internal sealed class UploadImagesCommandHandler(IApplicationDbContext context, IImageService imageService)
    : ICommandHandler<UploadImagesCommand>
{
    public async Task<Result> Handle(UploadImagesCommand command, CancellationToken cancellationToken)
    {
        Post? post = await context.Posts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.PostId, cancellationToken: cancellationToken);

        if (post is null)
        {
            return Result.Failure(PostErrors.NotFound(command.PostId));
        }

        List<string> urls = await imageService.UploadImagesAsync(command.Images, command.PostId);

        post.ImageUrls = urls;
        
        context.Posts.Update(post);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success("Image uploaded");
    }
}
