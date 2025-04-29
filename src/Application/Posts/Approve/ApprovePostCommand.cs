using Application.Abstractions.Messaging;

namespace Application.Posts.Approve;

public sealed record ApprovePostCommand(Guid PostId) : ICommand;
