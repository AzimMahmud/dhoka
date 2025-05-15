namespace Domain.Posts;

public record PostsResponse
{
    public Guid Id { get; init; }
    public string? Title { get; init; }
    public string? TransactionMode { get; init; }
    public string? PaymentType { get; init; }
    public string? Description { get; init; }
    public List<string>? MobilNumbers { get; init; }
    public decimal? Amount { get; init; }
    public string Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
