using Application.Users.Register;
using Application.Users.UpdateUser;
using Domain.Roles;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

public class UpdateUser : IEndpoint
{
    public sealed record Request(Guid Id, string FirstName, string LastName, string Email);

public void MapEndpoint(IEndpointRouteBuilder app)
{
    app.MapPut("users/{id:guid}", async (Guid id, Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            
            if (id != request.Id)
            {
                return Results.BadRequest("Id in the route must match the Id in the request body");
            }
            var command = new UpdateUserCommand(
                id,
                request.FirstName,
                request.LastName,
                request.Email);

            Result<Guid> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Users);
}
}
