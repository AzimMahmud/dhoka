namespace Application.Comments.GetById;

public record CommentResponse(
    Guid Id,
    Guid PostId,
    string ContactInfo,
    string Description,
    DateTime CreatedAt
);
