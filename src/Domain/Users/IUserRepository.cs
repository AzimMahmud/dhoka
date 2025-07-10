namespace Domain.Users;

public interface IUserRepository
{
    Task<User> GetByIdAsync(Guid id);
    Task<User> GetByEmailAsync(string email);

    Task<PagedResult<UsersResponse>> GetAllAsync(int pageSize, string? paginationToken, string statusFilter);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
}
