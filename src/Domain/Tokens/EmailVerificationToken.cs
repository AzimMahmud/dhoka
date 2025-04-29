using Domain.Users;
using SharedKernel;

namespace Domain.Tokens;

public sealed class EmailVerificationToken : Entity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime ExpiresOnUtc { get; set; }

    public User User { get; set; }
}
