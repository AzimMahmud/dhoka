using Application.Users.LoginWithRefreshToken;
using MediatR;
using SharedKernel;
using Web.Api.Endpoints;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

public class LoginWithRefreshToken : IEndpoint
{

    public sealed record Request(string RefreshToken);
    
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/refresh-token", async (Request request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new LoginWithRefreshTokenCommand(request.RefreshToken);

                Result<Response> result = await sender.Send(command, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithTags(Tags.Users);
    }
}
