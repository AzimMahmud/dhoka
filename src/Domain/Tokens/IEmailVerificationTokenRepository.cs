namespace Domain.Tokens;

public interface IEmailVerificationTokenRepository
{
    Task<EmailVerificationToken> GetByIdAsync(Guid id);

    Task CreateAsync(EmailVerificationToken token);
    Task UpdateAsync(EmailVerificationToken token);
    Task DeleteAsync(Guid id);
}
