namespace Application.Posts.GetById;

public record PostResponse(
    Guid Id,
    string? Title,
    string? TransactionMode,
    string? PaymentType,
    string? Description,
    List<string>? MobilNumbers,
    decimal? Amount,
    string Status
);
