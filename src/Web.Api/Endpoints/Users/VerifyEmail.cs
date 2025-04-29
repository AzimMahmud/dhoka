using Application.Posts.Verify;
using Application.Users.Register;
using Application.Users.VerifyEmail;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class VerifyEmail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/verify-email", async (Guid token, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new VerifyEmailCommand(token);

                Result<bool> res = await sender.Send(command, cancellationToken);

                return res.IsSuccess ? Results.Ok() : Results.Conflict(res.Error);
            })
            .WithTags(Tags.Users);
    }
}


