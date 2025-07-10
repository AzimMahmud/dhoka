using Application.Abstractions.Messaging;

namespace Application.Contacts.Delete;

public sealed record DeleteContactCommand(Guid Id) : ICommand;
