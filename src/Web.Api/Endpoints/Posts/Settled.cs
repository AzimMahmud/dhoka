using Application.Posts.Settled;
using Domain.Roles;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Posts;

internal sealed class Settled : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("posts/{id:guid}/settled", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new SettledPostCommand(id);

                Result result = await sender.Send(command, cancellationToken);

                return result.Match(Results.NoContent, CustomResults.Problem);
            })
            .WithTags(Tags.Posts)
            .RequireAuthorization(policy => policy.RequireRole(Role.Admin, Role.Member));
    }
}
