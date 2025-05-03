using Application.Posts.Get;
using Domain.Roles;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Posts;

internal sealed class AdminGet : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/posts", async (string? searchTerm,
                string? sortColumn,
                string? sortOrder,
                string status,
                int page,
                int pageSize,  ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new GetPostsQuery(searchTerm, sortColumn, sortOrder, status,page, pageSize);

            Result<PagedList<PostsResponse>> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Posts)
        .RequireAuthorization(policy => policy.RequireRole(Role.Admin, Role.Member));
        
    }
}
