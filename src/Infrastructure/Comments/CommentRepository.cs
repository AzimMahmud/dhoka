using System.Text;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Domain;
using Domain.Comments;
using Infrastructure.Database;

namespace Infrastructure.Comments;

public class CommentRepository(IAmazonDynamoDB dynamoDb) : ICommentRepository
{
 

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
            TableName = Tables.Comments,
            Item = item
        };

        await dynamoDb.PutItemAsync(request);
    }

    public async Task<CommentResponse?> GetByIdAsync(Guid id)
    {
        var request = new GetItemRequest
        {
            TableName = Tables.Comments,
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
        return new CommentResponse
        {
            Id = Guid.Parse(item["Id"].S),
            PostId = Guid.Parse(item["PostId"].S),
            ContactInfo = item["ContactInfo"].S,
            Description = item["Description"].S,
            CreatedAt = DateTime.Parse(item["CreatedAt"].S)
        };
    }

    public async Task<List<CommentsResponse>> GetByPostIdAsync(Guid postId)
    {
        // This requires a GSI (Global Secondary Index) on PostId

        var request = new QueryRequest
        {
            TableName = Tables.Comments,
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
            TableName = Tables.Comments,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id.ToString() }
            }
        };

        await dynamoDb.DeleteItemAsync(request);
    }

    public async Task<PagedResult<CommentsResponse>> GetByPostIdPaginatedAsync(
        Guid postId,
        int pageSize,
        string? paginationToken)
    {
        // Decode the pagination token to ExclusiveStartKey
        Dictionary<string, AttributeValue>? exclusiveStartKey = DecodePaginationToken(paginationToken);

        var request = new QueryRequest
        {
            TableName = Tables.Comments,
            IndexName = "PostId-index", // Ensure this GSI exists
            KeyConditionExpression = "PostId = :postId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":postId", new AttributeValue { S = postId.ToString() } }
            },
            Limit = pageSize,
            ExclusiveStartKey = exclusiveStartKey
        };

        QueryResponse? response = await dynamoDb.QueryAsync(request);

        var items = response.Items.Select(item => new CommentsResponse
        {
            Id = Guid.TryParse(item.GetValueOrDefault("Id")?.S, out Guid id) ? id : Guid.Empty,
            PostId = Guid.TryParse(item.GetValueOrDefault("PostId")?.S, out Guid post) ? post : Guid.Empty,
            ContactInfo = item.TryGetValue("ContactInfo", out AttributeValue? contact) ? contact.S : null,
            Description = item.TryGetValue("Description", out AttributeValue? desc) ? desc.S : null,
            Created = item.TryGetValue("CreatedAt", out AttributeValue? createdAt) &&
                      DateTime.TryParse(createdAt.S, out DateTime dt)
                ? dt
                : DateTime.MinValue
        }).ToList();

        return new PagedResult<CommentsResponse>
        {
            Items = items,
            NextPageToken = response.LastEvaluatedKey != null && response.LastEvaluatedKey.Count > 0
                ? EncodePaginationToken(response.LastEvaluatedKey)
                : null
        };
    }


    private Dictionary<string, AttributeValue>? DecodePaginationToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        string json = Encoding.UTF8.GetString(Convert.FromBase64String(token));
        return JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(json);
    }

    private string EncodePaginationToken(Dictionary<string, AttributeValue> token)
    {
        string json = JsonSerializer.Serialize(token);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }
}
