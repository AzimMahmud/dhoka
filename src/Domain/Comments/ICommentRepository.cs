namespace Domain.Comments;

public interface ICommentRepository
{
    Task CreateAsync(Comment comment);
    Task<CommentResponse?> GetByIdAsync(Guid id);
    Task<List<CommentsResponse>> GetByPostIdAsync(Guid postId);
    Task DeleteAsync(Guid id);

    Task<PagedResult<CommentsResponse>> GetByPostIdPaginatedAsync(Guid postId, int pageSize, string? lastEvaluatedKey);
}
