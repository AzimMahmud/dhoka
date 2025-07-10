using Application.Abstractions.Messaging;
using Domain.Tokens;
using Domain.Users;
using SharedKernel;

namespace Application.Users.VerifyEmail;

public sealed record VerifyEmailCommand(string Token) : ICommand<bool>;

internal class VerifyEmailCommandHandler(
    IUserRepository userRepository,
    IEmailVerificationTokenRepository emailVerificationTokenRepository) : ICommandHandler<VerifyEmailCommand, bool>
{
    public async Task<Result<bool>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Token, out Guid token))
        {
            return Result<bool>.ValidationFailure(UserErrors.TokenInvalid);
        }

        EmailVerificationToken? emailVerification = await emailVerificationTokenRepository.GetByIdAsync(token);


        User user = await userRepository.GetByIdAsync(emailVerification.UserId);

        if (emailVerification.ExpiresOnUtc < DateTime.UtcNow || user.EmailVerified)
        {
            return false;
        }

        user.EmailVerified = true;

        await emailVerificationTokenRepository.DeleteAsync(emailVerification.Id);

        await userRepository.UpdateAsync(user);

        return true;
    }
}
