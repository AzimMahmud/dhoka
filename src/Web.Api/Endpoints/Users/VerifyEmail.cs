using Application.Users.VerifyEmail;
using MediatR;
using SharedKernel;

namespace Web.Api.Endpoints.Users;

public record VerifyEmailRequest(string Token);

internal sealed class VerifyEmail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/verify-email", async (VerifyEmailRequest token, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new VerifyEmailCommand(token.Token);

                Result<bool> res = await sender.Send(command, cancellationToken);

                return res.IsSuccess ? Results.Ok() : Results.Conflict(res.Error);
            })
            .WithTags(Tags.Users);
    }
}


