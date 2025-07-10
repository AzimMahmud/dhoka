using Application.Abstractions.Messaging;

namespace Application.Contacts.Create;

public sealed class CreateContactCommand : ICommand<Guid>
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string MobileNumber { get; set; }
    public string Message { get; set; }
}
