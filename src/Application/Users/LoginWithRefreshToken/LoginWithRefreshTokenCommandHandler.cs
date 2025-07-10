using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Tokens;
using Domain.Users;
using SharedKernel;

namespace Application.Users.LoginWithRefreshToken;

public sealed record Response(string AccessToken, string RefreshToken);

internal sealed class LoginWithRefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    ITokenProvider tokenProvider) : ICommandHandler<LoginWithRefreshTokenCommand, Response>
{
    public async Task<Result<Response>> Handle(LoginWithRefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        RefreshToken? refreshToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken);


        if (refreshToken is null || refreshToken.ExpiresOnUtc < DateTime.UtcNow)
        {
            return Result<Response>.ValidationFailure(UserErrors.RefreshTokenInvalid);
        }

        User user = await userRepository.GetByIdAsync(refreshToken.UserId);

        string accessToken = await tokenProvider.CreateAsync(user);

        refreshToken.Token = tokenProvider.GenerateRefreshToken();

        refreshToken.ExpiresOnUtc = DateTime.UtcNow.AddDays(7);


        await refreshTokenRepository.UpdateAsync(refreshToken);

        return new Response(accessToken, refreshToken.Token);
    }
}
