using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Domain.Posts;

namespace Infrastructure.Posts;

public class PostCounterRepository(IAmazonDynamoDB dynamoDb) : IPostCounterRepository
{
    private readonly DynamoDBContext _context = new(dynamoDb);


    public async Task<int> GetCountAsync(string counterType)
    {
        PostCounter? counter = await _context.LoadAsync<PostCounter>(counterType);
        return counter?.Count ?? 0;
    }

    public async Task SetCountAsync(string counterType, int count)
    {
        var counter = new PostCounter
        {
            CounterType = counterType,
            Count = count
        };

        await _context.SaveAsync(counter);
    }

    public async Task IncrementAsync(string counterType, int delta)
    {
        var request = new UpdateItemRequest
        {
            TableName = "PostCounters",
            Key = new Dictionary<string, AttributeValue>
            {
                { "CounterType", new AttributeValue { S = counterType } }
            },
            UpdateExpression = "SET #c = if_not_exists(#c, :zero) + :delta",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#c", "Count" }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":delta", new AttributeValue { N = delta.ToString() } },
                { ":zero", new AttributeValue { N = "0" } }
            },
            ReturnValues = "UPDATED_NEW"
        };

        await dynamoDb.UpdateItemAsync(request);
    }
}
