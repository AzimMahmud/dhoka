using Application.Abstractions.Messaging;
using Domain;
using Domain.Users;

namespace Application.Users.Get;

public sealed record GetUsersQuery(int PageSize, string? PaginationToken, string Status) : IQuery<PagedResult<UsersResponse>>;
