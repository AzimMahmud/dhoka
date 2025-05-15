using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Domain;
using Domain.Comments;

namespace Infrastructure.Comments;

public class CommentRepository(IAmazonDynamoDB dynamoDb) : ICommentRepository
{
    private const string TableName = "Comments"; 

    public async Task CreateAsync(Comment comment)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = comment.Id.ToString() },
            ["PostId"] = new AttributeValue { S = comment.PostId.ToString() },
            ["ContactInfo"] = new AttributeValue { S = comment.ContactInfo },
            ["Description"] = new AttributeValue { S = comment.Description },
            ["CreatedAt"] = new AttributeValue { S = comment.CreatedAt.ToString("o") } // ISO 8601
        };

        var request = new PutItemRequest
        {
            TableName = TableName,
            Item = item
        };

        await dynamoDb.PutItemAsync(request);
    }

    public async Task<CommentsResponse?> GetByIdAsync(Guid id)
    {
        var request = new GetItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id.ToString() }
            }
        };

        GetItemResponse? response = await dynamoDb.GetItemAsync(request);

        if (!response.IsItemSet)
        {
            return null;
        }

        Dictionary<string, AttributeValue>? item = response.Item;
        return new CommentsResponse
        {
            Id = Guid.Parse(item["Id"].S),
            PostId = Guid.Parse(item["PostId"].S),
            ContactInfo = item["ContactInfo"].S,
            Description = item["Description"].S,
            Created = DateTime.Parse(item["CreatedAt"].S)
        };
    }

    public async Task<List<CommentsResponse>> GetByPostIdAsync(Guid postId)
    {
        // This requires a GSI (Global Secondary Index) on PostId

        var request = new QueryRequest
        {
            TableName = TableName,
            IndexName = "PostId-index", // Must match GSI name
            KeyConditionExpression = "PostId = :v_PostId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":v_PostId"] = new AttributeValue { S = postId.ToString() }
            }
        };

        QueryResponse? response = await dynamoDb.QueryAsync(request);

        return response.Items.Select(item => new CommentsResponse
        {
            Id = Guid.Parse(item["Id"].S),
            PostId = Guid.Parse(item["PostId"].S),
            ContactInfo = item["ContactInfo"].S,
            Description = item["Description"].S,
            Created = DateTime.Parse(item["CreatedAt"].S)
        }).ToList();
    }

    public async Task DeleteAsync(Guid id)
    {
        var request = new DeleteItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id.ToString() }
            }
        };

        await dynamoDb.DeleteItemAsync(request);
    }

    public async Task<PagedResult<CommentsResponse>> GetByPostIdPaginatedAsync(Guid postId, int pageSize, string? lastEvaluatedKey)
    {
        var request = new QueryRequest
        {
            TableName = "Comments",
            IndexName = "PostId-index", // GSI required
            KeyConditionExpression = "PostId = :postId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":postId"] = new AttributeValue { S = postId.ToString() }
            },
            Limit = pageSize
        };

        // Decode the last key if provided
        if (!string.IsNullOrEmpty(lastEvaluatedKey))
        {
            request.ExclusiveStartKey = JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(lastEvaluatedKey);
        }

        QueryResponse? response = await dynamoDb.QueryAsync(request);

        var comments = response.Items.Select(item => new CommentsResponse
        {
            Id = Guid.Parse(item["Id"].S),
            PostId = Guid.Parse(item["PostId"].S),
            ContactInfo = item["ContactInfo"].S,
            Description = item["Description"].S,
            Created = DateTime.Parse(item["CreatedAt"].S)
        }).ToList();

        // Encode LastEvaluatedKey for frontend or next call
        string? nextCursor = response.LastEvaluatedKey?.Count > 0
            ? JsonSerializer.Serialize(response.LastEvaluatedKey)
            : null;

        return new PagedResult<CommentsResponse>
        {
            Items = comments,
            PaginationToken = nextCursor
        };
    }
}
