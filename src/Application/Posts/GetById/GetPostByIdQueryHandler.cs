using Application.Abstractions.Messaging;
using Domain.Posts;
using Humanizer;
using SharedKernel;

namespace Application.Posts.GetById;

internal sealed class GetPostByIdQueryHandler(IPostRepository postRepository)
    : IQueryHandler<GetPostByIdQuery, PostResponse>
{
    public async Task<Result<PostResponse>> Handle(GetPostByIdQuery query, CancellationToken cancellationToken)
    {
        Post? post = await postRepository.GetByIdAsync(query.PostId);

        if (post.Id == Guid.Empty)
        {
            return Result.Failure<PostResponse>(PostErrors.NotFound(query.PostId));
        }

        return new PostResponse(
            post.Id,
            post.ScamType,
            post.Title,
            post.PaymentType,
            post.MobileNumbers,
            post.Amount,
            post.PaymentDetails,
            post.ScamDateTime,
            post.AnonymityPreference,
            post.Description,
            post.Status,
            post.CreatedAt.Humanize(false),
            post.ImageUrls
        );
    }
}
