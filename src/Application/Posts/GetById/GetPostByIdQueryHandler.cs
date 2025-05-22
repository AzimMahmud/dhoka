using Application.Abstractions.Messaging;
using Domain.Posts;
using SharedKernel;

namespace Application.Posts.GetById;

internal sealed class GetPostByIdQueryHandler(IPostRepository postRepository)
    : IQueryHandler<GetPostByIdQuery, PostResponse>
{
    public async Task<Result<PostResponse>> Handle(GetPostByIdQuery query, CancellationToken cancellationToken)
    {
        Post? post =  await postRepository.GetByIdAsync(query.PostId);
        
        if (post is null)
        {
            return Result.Failure<PostResponse>(PostErrors.NotFound(query.PostId));
        }

        return new PostResponse(
            post.Id,
            post.Title,
            post.TransactionMode,
            post.PaymentType,
            post.Description,
            post.MobilNumbers,
            post.Amount,
            post.Status
        );
    }
}
