namespace Domain.Contacts;

public interface IContactRepository
{
    Task CreateAsync(Contact contact);
    Task<ContactResponse?> GetByIdAsync(Guid id);
    Task<PagedResult<ContactResponse>> GetAllContactAsync(int pageSize, string? paginationToken);
    Task DeleteAsync(Guid id);
}
