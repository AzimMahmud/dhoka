using SharedKernel;

namespace Domain.Comments;

public static class CommentErrors
{
    public static Error AlreadyApproved(Guid commentId) => Error.Problem(
        "Comments.AlreadyApproved",
        $"The comment item with Id = '{commentId}' is already approved.");

    public static Error NotFound(Guid commentId) => Error.NotFound(
        "Comments.NotFound",
        $"The comment with the Id = '{commentId}' was not found");
    
    public static Error PostCommentsNotFound() => Error.NotFound(
        "Comments.PostCommentsNotFound",
        $"This post has no comments yet.");
}
