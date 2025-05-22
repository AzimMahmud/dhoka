using Amazon.DynamoDBv2.DataModel;

namespace Domain.Posts;

[DynamoDBTable("PostCounters")]
public class PostCounter
{
    [DynamoDBHashKey]
    public string CounterType { get; set; } // e.g., "ApprovedSettled"

    public int Count { get; set; }
}
