using Application.Posts.Get;
using Domain;
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
        app.MapGet("admin/posts",
                async (int pageSize, string? paginationToken, string status, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new GetPostsQuery(pageSize, paginationToken, status);

                    Result<PagedResult<PostsResponse>> result = await sender.Send(command, cancellationToken);

                    return result.Match(Results.Ok, CustomResults.Problem);
                })
            .WithTags(Tags.Posts).RequireAuthorization(policy => policy.RequireRole(Role.Admin, Role.Member));
    }
}
