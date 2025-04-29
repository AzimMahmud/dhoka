using Domain.Comments;
using Domain.Posts;
using Domain.Roles;
using Domain.SearchEvents;
using Domain.Tokens;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    
    DbSet<Post> Posts { get; }
    
    DbSet<Comment> Comments { get; }
    
    DbSet<SearchEvent> SearchEvents { get; }
    DbSet<Role> Roles { get; }
    
    public DbSet<UserRole> UserRoles { get; set; }
    
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
