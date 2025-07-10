using Application.Users.DeleteUser;
using Application.Users.UpdateUser;
using Domain.Roles;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

public class DeleteUser : IEndpoint
{
  

public void MapEndpoint(IEndpointRouteBuilder app)
{
    app.MapDelete("users/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new DeleteUserCommand(
                id);

            Result<Guid> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization(policy => policy.RequireRole(Role.Admin))
        .WithTags(Tags.Users);
}
}
