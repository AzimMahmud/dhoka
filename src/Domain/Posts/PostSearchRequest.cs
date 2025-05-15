namespace Domain.Posts;

public class PostSearchRequest
{
    public string? SearchTerm { get; set; }
    public int PageSize { get; set; } = 10;
    public string? LastEvaluatedKey { get; set; }
}
