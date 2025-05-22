using Application.Comments.Get;
using Domain;
using Domain.Comments;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Comments;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("comments", async (Guid postId,
                int pageSize, string paginationToken, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new GetCommentsQuery(postId,  pageSize, paginationToken);

            Result<PagedResult<CommentsResponse>> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Comments);
    }
}
