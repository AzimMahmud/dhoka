

using Application.Abstractions.Messaging;

namespace Application.Users.RevokeRefreshTokens;

public sealed record RevokeRefreshTokenCommand(Guid UserId): ICommand<bool>;

