namespace Domain.Posts;

public interface IPostRepository
{
    Task<Post> GetByIdAsync(Guid id);

    Task<List<PostsResponse>> GetRecentPostsAsync();
    Task<PagedResult<PostsResponse>> GetAllAsync(int pageSize, string? paginationToken,string? statusFilter = null);
    Task CreateAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Guid id);
    Task<PagedSearchResult<PostsResponse>> SearchAsync(PostSearchRequest request);

    Task<List<string?>> AutocompleteTitlesAsync(AutocompleteRequest request);
}
