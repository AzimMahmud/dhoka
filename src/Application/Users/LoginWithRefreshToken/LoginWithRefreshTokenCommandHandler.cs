using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tokens;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.LoginWithRefreshToken;

public sealed record Response(string AccessToken, string RefreshToken);

internal sealed class LoginWithRefreshTokenCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider) : ICommandHandler<LoginWithRefreshTokenCommand, Response>
{
    public async Task<Result<Response>> Handle(LoginWithRefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        RefreshToken? refreshToken = await context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken, cancellationToken: cancellationToken);

        if (refreshToken is null || refreshToken.ExpiresOnUtc < DateTime.UtcNow)
        {
            throw new ApplicationException("The refresh token has expired");
        }

        string accessToken = await tokenProvider.CreateAsync(refreshToken.User);

        refreshToken.Token = tokenProvider.GenerateRefreshToken();

        refreshToken.ExpiresOnUtc = DateTime.UtcNow.AddDays(7);

        await context.SaveChangesAsync(cancellationToken);

        return new Response(accessToken, refreshToken.Token);
    }
}
