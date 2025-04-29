using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tokens;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Login;


    
public sealed record Response(string AccessToken, string RefreshToken);

internal sealed class LoginUserCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider) : ICommandHandler<LoginUserCommand, Response>
{
    public async Task<Result<Response>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await context.Users.Where(x => x.Email == request.Email).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (user is null || !user.EmailVerified)
        {
            throw new ApplicationException("The user was not found");
        }

        bool verified = passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!verified)
        {
            throw new ApplicationException("The password is incorrect");
        }

        string token = await tokenProvider.CreateAsync(user);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = tokenProvider.GenerateRefreshToken(),
            ExpiresOnUtc = DateTime.UtcNow.AddDays(7)
        };

        context.RefreshTokens.Add(refreshToken);

        await context.SaveChangesAsync();

        return new Response(token, refreshToken.Token);
    }
}
