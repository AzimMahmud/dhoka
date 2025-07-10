using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Tokens;
using Domain.Users;
using SharedKernel;

namespace Application.Users.Login;


    
public sealed record Response(string AccessToken, string RefreshToken);

internal sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider) : ICommandHandler<LoginUserCommand, Response>
{
    public async Task<Result<Response>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByEmailAsync(request.Email);
        
        if (user is null || !user.EmailVerified || user.Status != nameof(Status.Active))
        {
            return Result.Failure<Response>(UserErrors.Unauthorized);
        }

        bool verified = passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!verified)
        {
            return Result.Failure<Response>(UserErrors.PasswordNotMatch);
        }

        string token = await tokenProvider.CreateAsync(user);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = tokenProvider.GenerateRefreshToken(),
            ExpiresOnUtc = DateTime.UtcNow.AddDays(7)
        };

        await refreshTokenRepository.CreateAsync(refreshToken);
        
        return new Response(token, refreshToken.Token);
    }
}
