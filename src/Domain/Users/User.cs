using SharedKernel;

namespace Domain.Users;

public sealed class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PasswordHash { get; set; }
    public bool EmailVerified { get; set; }
    
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Role { get; set; }
}
