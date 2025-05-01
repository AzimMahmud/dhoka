using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Posts.GetById;

internal sealed class GetPostByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetPostByIdQuery, PostResponse>
{
    public async Task<Result<PostResponse>> Handle(GetPostByIdQuery query, CancellationToken cancellationToken)
    {
        PostResponse? post = await context.Posts
            .Where(todoItem => todoItem.Id == query.PostId)
            .AsNoTracking()
            .Select(post => new PostResponse(
                    post.Id,
                    post.Title,
                    post.TransactionMode,
                    post.PaymentType,
                    post.Description,
                    post.MobilNumbers,
                    post.Amount,
                    post.Status
                ))
                .SingleOrDefaultAsync(cancellationToken);

        if (post is null)
        {
            return Result.Failure<PostResponse>(PostErrors.NotFound(query.PostId));
        }

        return post;
    }
}
