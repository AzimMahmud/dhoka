using Application.Abstractions.Messaging;

namespace Application.Posts.Admin.Reject;

public sealed record RejectPostCommand(Guid PostId) : ICommand;
