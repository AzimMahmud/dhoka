using SharedKernel;

namespace Domain.Posts;

public static class PostErrors
{
    public static Error AlreadyApproved(Guid postId) => Error.Problem(
        "Posts.AlreadyApproved",
        $"The post item with Id = '{postId}' is already approved.");

    public static Error NotFound(Guid postId) => Error.NotFound(
        "Posts.NotFound",
        $"The post with the Id = '{postId}' was not found");
    
    public static Error NotApproved(Guid postId) => Error.Conflict(
        "Posts.NotApproved",
        $"The post with the Id = '{postId}' was not approve yet"); 
    
    public static Error ImageNotUploaded(Guid postId) => Error.Conflict(
        "Posts.ImageNotUploaded",
        $"The images with the Id = '{postId}' was not Uploaded");
}
