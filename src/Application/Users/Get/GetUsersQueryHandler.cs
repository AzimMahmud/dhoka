using Application.Abstractions.Messaging;
using Domain;
using Domain.Users;
using SharedKernel;

namespace Application.Users.Get;

internal sealed class GetUsersQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetUsersQuery, PagedResult<UsersResponse>>
{
    public async Task<Result<PagedResult<UsersResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await userRepository.GetAllAsync(request.PageSize, request.PaginationToken, request.Status);
    }
}
