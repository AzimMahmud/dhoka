using Application.Comments.Create;
using Application.Contacts.Create;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Contacts;

internal sealed class Create : IEndpoint
{
    public sealed class Request
    {
    
        public string Name { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string Message { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("contacts", async (Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new CreateContactCommand
            {
               Name = request.Name,
               Email = request.Email,
               MobileNumber = request.MobileNumber,
               Message = request.Message
            };

            Result<Guid> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Contacts);
    }
}
