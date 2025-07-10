using Application.Abstractions.Messaging;
using Domain;
using Domain.Comments;
using Domain.Contacts;
using SharedKernel;

namespace Application.Contacts.Get;

internal sealed class GetContactsQueryHandler(IContactRepository contactRepository)
    : IQueryHandler<GetContactsQuery, PagedResult<ContactResponse>>
{
    public async Task<Result<PagedResult<ContactResponse>>> Handle(GetContactsQuery request,
        CancellationToken cancellationToken)
    {
        PagedResult<ContactResponse> contacts =
            await contactRepository.GetAllContactAsync(request.PageSize, request.PaginationToken);

        return contacts;
    }
}
