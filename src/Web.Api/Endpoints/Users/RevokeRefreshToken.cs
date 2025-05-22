using Application.Users.RevokeRefreshTokens;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class RevokeRefreshToken : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("users/{id:guid}/refresh-tokens", async (Guid id, ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new RevokeRefreshTokenCommand(id);

                Result<bool> result = await sender.Send(command, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithTags(Tags.Users)
            .RequireAuthorization();
    }
}
