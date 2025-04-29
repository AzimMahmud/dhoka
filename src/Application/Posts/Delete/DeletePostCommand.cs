using Application.Abstractions.Messaging;

namespace Application.Posts.Delete;

public sealed record DeletePostCommand(Guid PostId) : ICommand;
