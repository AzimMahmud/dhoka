using Application.Abstractions.Messaging;

namespace Application.Posts.SendToken;

public sealed record SendTokenCommand(Guid PostId, string PhoneNumber) : ICommand<int>;
