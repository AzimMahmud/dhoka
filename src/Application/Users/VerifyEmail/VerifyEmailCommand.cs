using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tokens;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.VerifyEmail;

public sealed record VerifyEmailCommand(string Token) : ICommand<bool>;

internal class VerifyEmailCommandHandler(IApplicationDbContext context) : ICommandHandler<VerifyEmailCommand, bool>
{
    public async Task<Result<bool>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Token, out Guid token))
        {
            return Result<bool>.ValidationFailure(UserErrors.TokenInvalid);
        }

        EmailVerificationToken? emailVerification = await context.EmailVerificationTokens
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == token, cancellationToken: cancellationToken);

        if (emailVerification is null || emailVerification.ExpiresOnUtc < DateTime.UtcNow ||
            emailVerification.User.EmailVerified)
        {
            return false;
        }

        emailVerification.User.EmailVerified = true;

        context.EmailVerificationTokens.Remove(emailVerification);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
