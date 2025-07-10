using Application.Comments.GetById;
using Application.Contacts.GetById;
using Domain.Comments;
using Domain.Contacts;
using Domain.Roles;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Contacts;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("contacts/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new GetContactByIdQuery(id);

            Result<ContactResponse> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization(policy => policy.RequireRole(Role.Admin, Role.Member))
        .WithTags(Tags.Contacts);
    }
}
