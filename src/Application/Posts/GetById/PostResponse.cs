namespace Application.Posts.GetById;

public record PostResponse(
    Guid Id,
    string? ScamType,
    string? Title,
    string? PaymentType,
    List<string>? MobileNumbers,
    decimal? Amount,
    string? PaymentDetails,
    DateTime? ScamDateTime,
    string? AnonymityPreference,
    string? Description,
    string? Status,
    string CreatedAt,
    List<string>? ImageUrls
);
