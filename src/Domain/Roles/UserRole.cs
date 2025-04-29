using Domain.Users;
using SharedKernel;

namespace Domain.Roles;

public sealed class UserRole : Entity
{
    public Guid UserId { get; set; }
    public int RoleId { get; set; }

    public User User { get; set; }
    public Role Role { get; set; }
}
