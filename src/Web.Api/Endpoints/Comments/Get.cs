using Application.Comments.Get;
using Application.Posts.Get;
using Application.Posts.GetById;
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
                string? sortColumn,
                string? sortOrder,
                int page,
                int pageSize, string paginationToken, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new GetCommentsQuery(postId, sortColumn, sortOrder, page, pageSize, paginationToken);

            Result<PagedResult<CommentsResponse>> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Comments);
    }
}
