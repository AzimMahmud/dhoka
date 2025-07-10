using Application.Abstractions.Messaging;

namespace Application.Posts.Admin.ReIndex;

public sealed record ReIndexPostCommand(Guid PostId) : ICommand;
