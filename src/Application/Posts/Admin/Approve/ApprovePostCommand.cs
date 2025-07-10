using Application.Abstractions.Messaging;

namespace Application.Posts.Admin.Approve;

public sealed record ApprovePostCommand(Guid PostId) : ICommand;
