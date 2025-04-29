using Application.Abstractions.Messaging;

namespace Application.Posts.Init;

public sealed class  InitPostCommand : ICommand<Guid>;
