using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Roles.GetRoles;

internal sealed class GetRolesQueryHandler(IApplicationDbContext context) : IQueryHandler<GetRolesQuery, List<RoleResponse>>
{
    public async Task<Result<List<RoleResponse>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        return await context.Roles
            .Select(r => new RoleResponse(r.Id, r.Name))
            .ToListAsync(cancellationToken);
    }
}
