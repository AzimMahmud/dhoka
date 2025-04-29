namespace Application.Comments.Get;

public record CommentsResponse(
    Guid Id,
    Guid PostId,
    string ContactInfo,
    string Description
);
