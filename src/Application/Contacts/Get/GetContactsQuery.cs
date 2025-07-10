using Application.Abstractions.Messaging;
using Domain;
using Domain.Comments;
using Domain.Contacts;

namespace Application.Contacts.Get;

public sealed record GetContactsQuery(int PageSize, string? PaginationToken) : IQuery<PagedResult<ContactResponse>>;
