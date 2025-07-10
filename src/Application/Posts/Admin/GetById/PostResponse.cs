namespace Application.Posts.Admin.GetById;

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
    string? Name,
    string? ContactNumber,
    string? Status,
    DateTime? CreatedAt,
    List<string>? ImageUrls
);
