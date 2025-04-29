using Application.Abstractions.Messaging;

namespace Application.Posts.Settled;

public sealed record SettledPostCommand(Guid PostId) : ICommand;
