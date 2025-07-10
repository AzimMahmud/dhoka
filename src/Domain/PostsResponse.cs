namespace Domain;

public record PostsResponse
{
    public Guid Id { get; init; }
    public string? ScamType { get; init; }
    public string? Title { get; init; }
    public string? Status { get; init; }
    public string? Description { get; init; }
    public string? PaymentType { get; init; }
    public string? Anonymity { get; init; }
    public string? PaymentDetails { get; init; }
    public DateTime? ScamDateTime { get; init; }
    public decimal? Amount { get; init; }
    public DateTime? CreatedAt { get; init; }
}
