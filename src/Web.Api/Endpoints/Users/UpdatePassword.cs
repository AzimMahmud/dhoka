using Application.Users.UpdatePassword;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

public class UpdatePassword : IEndpoint
{
    public sealed record Request(Guid Id, string Password, string ConfirmedPassword);

public void MapEndpoint(IEndpointRouteBuilder app)
{
    app.MapPut("users/{id:guid}/change-password", async (Guid id, Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            
            if (id != request.Id)
            {
                return Results.BadRequest("Id in the route must match the Id in the request body");
            }
            var command = new UpdatePasswordCommand(
                id,
                request.Password,
                request.ConfirmedPassword);

            Result<Guid> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Users);
}
}
