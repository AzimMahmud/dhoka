using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Posts.Delete;

internal sealed class DeletePostCommandHandler(IApplicationDbContext context, IImageService imageService)
    : ICommandHandler<DeletePostCommand>
{
    public async Task<Result> Handle(DeletePostCommand command, CancellationToken cancellationToken)
    {
        Post? post = await context.Posts
            .SingleOrDefaultAsync(t => t.Id == command.PostId, cancellationToken);

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

        context.Posts.Remove(post);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
