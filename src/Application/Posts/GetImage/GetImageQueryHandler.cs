using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Posts.GetImage;

internal sealed class GetImageQueryHandler(IPostRepository postRepository) : IQueryHandler<GetImageQuery, List<string>>
{
    public async Task<Result<List<string>>> Handle(GetImageQuery query, CancellationToken cancellationToken)
    {
        Post? post =  await postRepository.GetByIdAsync(query.PostId);
        
        // List<string>? imageUrls = await context.Posts.Where(x => x.Id == request.PostId).AsNoTracking().Select(x => x.ImageUrls)
        //     .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return Result.Success(post.ImageUrls ?? new List<string>());
    }
}
