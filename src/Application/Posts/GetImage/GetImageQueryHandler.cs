using Application.Abstractions.Messaging;
using Domain.Posts;
using SharedKernel;

namespace Application.Posts.GetImage;

internal sealed class GetImageQueryHandler(IPostRepository postRepository) : IQueryHandler<GetImageQuery, List<string>>
{
    public async Task<Result<List<string>>> Handle(GetImageQuery query, CancellationToken cancellationToken)
    {
        Post? post =  await postRepository.GetByIdAsync(query.PostId);
        
        return Result.Success(post.ImageUrls ?? new List<string>());
    }
}
