using Domain.Roles;
using Domain.Tokens;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    
    DbSet<Role> Roles { get; }
    
    public DbSet<UserRole> UserRoles { get; set; }
    
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
