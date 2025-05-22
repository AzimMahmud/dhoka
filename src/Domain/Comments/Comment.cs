using Amazon.DynamoDBv2.DataModel;

namespace Domain.Comments;

[DynamoDBTable("Comments")]
public class Comment
{
    [DynamoDBHashKey]
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string ContactInfo { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
