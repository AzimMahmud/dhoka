using Application.Posts.Search;
using Domain;
using Domain.Posts;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Posts;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("posts/search", async ([AsParameters]PostSearchRequest request,   ISender sender, CancellationToken cancellationToken) =>
            {
                var query = new SearchPostsQuery(request);

                Result<PagedResult<PostsResponse>> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithTags(Tags.Posts);
    }
}
