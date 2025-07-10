using Application.Abstractions.Messaging;
using Domain.Comments;
using Domain.Contacts;

namespace Application.Contacts.GetById;

public sealed record GetContactByIdQuery(Guid Id) : IQuery<ContactResponse>;
