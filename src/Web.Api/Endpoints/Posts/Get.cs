using Application.Posts.Search;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Posts;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("posts/search", async (string? searchTerm,
                string? sortColumn,
                string? sortOrder,
                int page,
                int pageSize, ISender sender, CancellationToken cancellationToken) =>
            {
                var query = new SearchPostsQuery(searchTerm, sortColumn, sortOrder, page, pageSize);

                Result<SearchPagedList> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithTags(Tags.Posts);
    }
}
