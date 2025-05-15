namespace Domain.Posts;

public interface IPostRepository
{
    Task<Post> GetByIdAsync(Guid id);
    Task<IEnumerable<Post>> GetAllAsync();
    Task CreateAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Guid id);
    Task<PagedResult<PostsResponse>> SearchAsync(PostSearchRequest request);

    Task<List<string>> AutocompleteTitlesAsync(AutocompleteRequest request);
}
