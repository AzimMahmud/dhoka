using Application.Abstractions.Messaging;

namespace Application.Posts.Admin.Delete;

public sealed record DeletePostCommand(Guid PostId) : ICommand;
