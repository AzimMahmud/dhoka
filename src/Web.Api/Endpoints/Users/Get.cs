using Application.Users.Get;
using Domain.Roles;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users", async (string? searchTerm,
                string? sortColumn,
                string? sortOrder,
                int page,
                int pageSize, ISender sender, CancellationToken cancellationToken) =>
        {
            var query = new GetUsersQuery(searchTerm, sortColumn, sortOrder, page, pageSize);

            Result<PagedList<UsersResponse>> result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization(policy => policy.RequireRole(Role.Admin))
        .WithTags(Tags.Users);
    }
}
