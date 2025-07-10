using Application.Posts.Admin.Approve;
using Domain.Roles;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Posts;

internal sealed class Approve : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("posts/{id:guid}/approve", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new ApprovePostCommand(id);

                Result result = await sender.Send(command, cancellationToken);

                return result.Match(Results.NoContent, CustomResults.Problem);
            })
            .WithTags(Tags.Posts)
            .RequireAuthorization(policy => policy.RequireRole(Role.Admin, Role.Member));
    }
}
