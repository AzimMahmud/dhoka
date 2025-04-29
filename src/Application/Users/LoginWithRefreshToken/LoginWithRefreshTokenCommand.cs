using Application.Abstractions.Messaging;

namespace Application.Users.LoginWithRefreshToken;

public sealed record LoginWithRefreshTokenCommand(string RefreshToken) : ICommand<Response>;
