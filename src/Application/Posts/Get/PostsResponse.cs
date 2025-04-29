namespace Application.Posts.Get;

public record PostsResponse(
    Guid Id,
    string? Title,
    string? TransactionMode,
    string? PaymentType,
    string? Description,
    List<string>? MobilNumbers,
    decimal? Amount,
    string Status,
    DateTime CreatedAt
);
