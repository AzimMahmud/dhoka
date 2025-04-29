using Application.Roles.GetRoles;
using Domain.Roles;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Roles;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("roles", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new GetRolesQuery();

                Result<List<RoleResponse>> result = await sender.Send(command, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithTags(Tags.Roles)
            .RequireAuthorization(policy => policy.RequireRole(Role.Admin));
    }
}
