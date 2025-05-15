namespace Domain.Comments;

public record CommentsResponse
{
    public Guid Id { get; init; } 
    public Guid PostId { get; init; } 
    public string ContactInfo { get; init; } 
    public DateTime Created { get; init; } 
    public string Description { get; init; } 
}
