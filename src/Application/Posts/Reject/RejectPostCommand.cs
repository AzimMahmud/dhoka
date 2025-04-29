using Application.Abstractions.Messaging;

namespace Application.Posts.Reject;

public sealed record RejectPostCommand(Guid PostId) : ICommand;
