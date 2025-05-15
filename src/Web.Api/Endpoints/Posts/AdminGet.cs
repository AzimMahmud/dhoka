using Application.Posts.Get;
using Domain;
using Domain.Posts;
using Domain.Roles;
using Infrastructure.Posts;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Posts;

internal sealed class AdminGet : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/posts", async ([AsParameters]PostSearchRequest request,  ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new GetPostsQuery(request);

            Result<PagedResult<PostsResponse>> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Posts)
        .RequireAuthorization(policy => policy.RequireRole(Role.Admin, Role.Member));
        
    }
}
