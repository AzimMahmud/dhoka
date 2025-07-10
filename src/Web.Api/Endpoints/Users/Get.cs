using Amazon.CloudFront.Model;
using Application.Users.Get;
using Domain;
using Domain.Roles;
using Domain.Users;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users", async (int pageSize, string? paginationToken, string status, ISender sender, CancellationToken cancellationToken) =>
        {
            var query = new GetUsersQuery(pageSize, paginationToken, status);

            Result<PagedResult<UsersResponse>> result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization(policy => policy.RequireRole(Role.Admin))
        .WithTags(Tags.Users);
    }
}
