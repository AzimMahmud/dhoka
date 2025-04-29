using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tokens;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.VerifyEmail;

public sealed record VerifyEmailCommand(Guid Token) : ICommand<bool>;

internal class VerifyEmailCommandHandler(IApplicationDbContext context) : ICommandHandler<VerifyEmailCommand, bool>
{
    public async Task<Result<bool>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        EmailVerificationToken? token = await context.EmailVerificationTokens
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == request.Token, cancellationToken: cancellationToken);

        if (token is null || token.ExpiresOnUtc < DateTime.UtcNow || token.User.EmailVerified)
        {
            return false;
        }

        token.User.EmailVerified = true;

        context.EmailVerificationTokens.Remove(token);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
