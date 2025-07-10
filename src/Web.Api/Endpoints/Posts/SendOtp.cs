using Application.Posts.SendToken;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Posts;

internal sealed class SendOtp : IEndpoint
{
    public sealed class Request
    {
        public string PhoneNumber { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("posts/{id:guid}/send-otp",
                async (Guid id, Request request, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new SendTokenCommand(id, request.PhoneNumber);

                    Result<bool> result = await sender.Send(command, cancellationToken);

                    return result.Match(Results.Ok, CustomResults.Problem);
                })
            .WithTags(Tags.Posts);
    }
}
