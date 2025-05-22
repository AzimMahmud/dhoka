namespace Domain.Comments;

public record CommentResponse
{
    public Guid Id { get; init; } 
    public Guid PostId { get; init; } 
    public string ContactInfo { get; init; } 
    public string Description { get; init; } 
    public DateTime CreatedAt { get; init; } 
}
