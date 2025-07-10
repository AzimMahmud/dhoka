using Application.Comments.Get;
using Application.Contacts.Get;
using Domain;
using Domain.Comments;
using Domain.Contacts;
using Domain.Roles;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Contacts;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("contacts",
                async (int pageSize, string? paginationToken, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new GetContactsQuery(pageSize, paginationToken);

                    Result<PagedResult<ContactResponse>> result = await sender.Send(command, cancellationToken);

                    return result.Match(Results.Ok, CustomResults.Problem);
                })
            .RequireAuthorization(policy => policy.RequireRole(Role.Admin, Role.Member))
            .WithTags(Tags.Contacts);
    }
}
