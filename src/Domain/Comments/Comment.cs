using SharedKernel;

namespace Domain.Comments;

public class Comment : Entity
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string ContactInfo { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
