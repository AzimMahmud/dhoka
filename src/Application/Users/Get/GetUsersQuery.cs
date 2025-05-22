using Application.Abstractions.Messaging;

namespace Application.Users.Get;

public sealed record GetUsersQuery(string? SearchTerm,
    string? SortColumn,
    string? SortOrder,
    int Page,
    int PageSize) : IQuery<PagedList<UsersResponse>>;
