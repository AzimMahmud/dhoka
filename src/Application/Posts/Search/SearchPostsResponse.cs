namespace Application.Posts.Search;

public record SearchPostsResponse
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? TransactionMode { get; set; }
    public string? PaymentType { get; set; }
    public string? Description { get; set; }
    public List<string>? MobilNumbers { get; set; }
    public decimal? Amount { get; set; }
    internal DateTime CreatedAt { get; set; }

    public string Created { get; set; }
};
