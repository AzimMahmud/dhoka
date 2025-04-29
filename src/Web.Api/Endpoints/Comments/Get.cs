using Application.Comments.Get;
using Application.Posts.Get;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Comments;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("comments", async (string? searchTerm,
                string? sortColumn,
                string? sortOrder,
                int page,
                int pageSize, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new GetCommentsQuery(searchTerm, sortColumn, sortOrder, page, pageSize);

            Result<PagedList<CommentsResponse>> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Comments);
    }
}
