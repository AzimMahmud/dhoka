using Application.Abstractions.Messaging;
using Domain.Comments;
using Domain.Contacts;
using SharedKernel;

namespace Application.Contacts.Create;

internal sealed class CreateContactCommandHandler(
    IContactRepository contactRepository)
    : ICommandHandler<CreateContactCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateContactCommand command, CancellationToken cancellationToken)
    {
        var contact = new Contact()
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Email = command.Email,
            MobileNumber = command.MobileNumber,
            Message = command.Message,
            CreatedAt = DateTime.UtcNow
        };
   
        await contactRepository.CreateAsync(contact);
        return contact.Id;
    }
}
