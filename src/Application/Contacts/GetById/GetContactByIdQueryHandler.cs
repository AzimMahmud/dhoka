using Application.Abstractions.Messaging;
using Domain.Comments;
using Domain.Contacts;
using SharedKernel;

namespace Application.Contacts.GetById;

internal sealed class GetContactByIdQueryHandler(IContactRepository contactRepository)
    : IQueryHandler<GetContactByIdQuery, ContactResponse>
{
    public async Task<Result<ContactResponse>> Handle(GetContactByIdQuery query, CancellationToken cancellationToken)
    {
        ContactResponse? contact = await contactRepository.GetByIdAsync(query.Id);

        return contact ?? Result.Failure<ContactResponse>(ContactErrors.NotFound(query.Id));
    }
}
