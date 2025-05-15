using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain;
using Domain.Posts;

namespace Infrastructure.Posts;

public class PostRepository : IPostRepository
{
    private readonly IAmazonDynamoDB _dynamoDB;
    private readonly string TableName = "Posts";

    public PostRepository(IAmazonDynamoDB dynamoDB)
    {
        _dynamoDB = dynamoDB;
    }

    public async Task<Post?> GetByIdAsync(Guid id)
    {
        var request = new GetItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id.ToString() }
            }
        };

        GetItemResponse? response = await _dynamoDB.GetItemAsync(request);
        if (response.Item == null || response.Item.Count == 0)
        {
            return null;
        }

        return MapFromDynamoDb(response.Item);
    }

    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        ScanResponse? response = await _dynamoDB.ScanAsync(new ScanRequest { TableName = TableName });

        return response.Items.Select(MapFromDynamoDb);
    }

    public async Task CreateAsync(Post post)
    {
        var request = new PutItemRequest
        {
            TableName = TableName,
            Item = MapToDynamoDb(post)
        };

        await _dynamoDB.PutItemAsync(request);
    }

    public async Task UpdateAsync(Post post)
    {
        // Overwrite the entire item
        var request = new PutItemRequest
        {
            TableName = TableName,
            Item = MapToDynamoDb(post)
        };

        await _dynamoDB.PutItemAsync(request);
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

        await _dynamoDB.DeleteItemAsync(request);
    }

    public async Task<PagedResult<PostsResponse>> SearchAsync(PostSearchRequest request)
    {
        var scanRequest = new ScanRequest
        {
            TableName = "Posts",
            Limit = request.PageSize,
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>(),
            ExpressionAttributeNames = new Dictionary<string, string>()
        };

        var filterExpressions = new List<string>();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            string search = request.SearchTerm;

            string[] fields = new[]
                { "Title", "Description", "TransactionMode", "PaymentType", "ContactNumber", "Status" };

            int i = 0;
            foreach (string field in fields)
            {
                string valueKey = $":val{i}";
                string nameKey = $"#{field}";

                scanRequest.ExpressionAttributeValues[valueKey] = new AttributeValue { S = search };
                scanRequest.ExpressionAttributeNames[nameKey] = field;

                filterExpressions.Add($"contains({nameKey}, {valueKey})");
                i++;
            }

            scanRequest.FilterExpression = string.Join(" OR ", filterExpressions);
        }

        if (!string.IsNullOrEmpty(request.LastEvaluatedKey))
        {
            scanRequest.ExclusiveStartKey =
                JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(request.LastEvaluatedKey);
        }


        ScanResponse? response = await _dynamoDB.ScanAsync(scanRequest);

        var posts = response.Items.Select(item => new PostsResponse
        {
            Id = Guid.Parse(item["Id"].S),
            Title = item.TryGetValue("Title", out AttributeValue? title) ? title.S : null,
            TransactionMode = item.TryGetValue("TransactionMode", out AttributeValue? tm) ? tm.S : null,
            PaymentType = item.TryGetValue("PaymentType", out AttributeValue? pt) ? pt.S : null,
            Description = item.TryGetValue("Description", out AttributeValue? desc) ? desc.S : null,
            MobilNumbers = item.TryGetValue("MobilNumbers", out AttributeValue? mn) ? mn.SS : new List<string>(),
            Amount = item.TryGetValue("Amount", out AttributeValue? amt) ? decimal.Parse(amt.N) : null,
            Status = item["Status"].S,
        }).ToList();

        return new PagedResult<PostsResponse>
        {
            Items = posts,
            PaginationToken = response.LastEvaluatedKey?.Count > 0
                ? JsonSerializer.Serialize(response.LastEvaluatedKey)
                : null
        };
    }

    public async Task<List<string>> AutocompleteTitlesAsync(AutocompleteRequest request)
    {
        var scanRequest = new ScanRequest
        {
            TableName = "Posts",
            Limit = request.MaxResults, // You can fetch more and filter if needed
            FilterExpression = "begins_with(#title, :prefix)",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#title", "Title" }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":prefix", new AttributeValue { S = request.Prefix } }
            },
            ProjectionExpression = "#title"
        };

        ScanResponse? response = await _dynamoDB.ScanAsync(scanRequest);

        return response.Items
            .Select(i => i["Title"].S)
            .Distinct()
            .Take(request.MaxResults)
            .ToList();
    }

    // Helper: map from DynamoDB attributes to Product
    private static Post MapFromDynamoDb(Dictionary<string, AttributeValue> item)
    {
        return new Post
        {
            Id = Guid.Parse(item["Id"].S),
            Title = item.GetValueOrDefault("Title")?.S,
            TransactionMode = item.GetValueOrDefault("TransactionMode")?.S,
            PaymentType = item.GetValueOrDefault("PaymentType")?.S,
            Description = item.GetValueOrDefault("Description")?.S,
            MobilNumbers = item.GetValueOrDefault("MobilNumbers")?.SS?.ToList() ?? [],
            Amount = item.GetValueOrDefault("Amount") != null ? decimal.Parse(item["Amount"].N) : null,
            Status = item.GetValueOrDefault("Status")?.S ?? "",
            IsSettled = item.GetValueOrDefault("IsSettled")?.BOOL ?? false,
            ContactNumber = item.GetValueOrDefault("ContactNumber")?.S,
            Otp = item.GetValueOrDefault("Otp")?.N is string otpStr ? int.Parse(otpStr) : 0,
            OtpExpirationTime = item.GetValueOrDefault("OtpExpirationTime")?.S != null
                ? DateTime.Parse(item["OtpExpirationTime"].S)
                : null,
            CreatedAt = item.GetValueOrDefault("CreatedAt")?.S != null
                ? DateTime.Parse(item["CreatedAt"].S)
                : DateTime.UtcNow,
            ImageUrls = item.GetValueOrDefault("ImageUrls")?.SS?.ToList() ?? []
        };
    }

    // Helper: map Product to DynamoDB attributes
    private static Dictionary<string, AttributeValue> MapToDynamoDb(Post post)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = post.Id.ToString() },
            ["Title"] = new AttributeValue { S = post.Title ?? string.Empty },
            ["TransactionMode"] = new AttributeValue { S = post.TransactionMode ?? string.Empty },
            ["PaymentType"] = new AttributeValue { S = post.PaymentType ?? string.Empty },
            ["Description"] = new AttributeValue { S = post.Description ?? string.Empty },
            ["Amount"] = post.Amount.HasValue ? new AttributeValue { N = post.Amount.Value.ToString() } : null,
            ["Status"] = new AttributeValue { S = post.Status ?? string.Empty },
            ["IsSettled"] = new AttributeValue { BOOL = post.IsSettled ?? false },
            ["ContactNumber"] = new AttributeValue { S = post.ContactNumber ?? string.Empty },
            ["Otp"] = new AttributeValue { N = post.Otp.ToString() ?? string.Empty },
            ["OtpExpirationTime"] = post.OtpExpirationTime.HasValue
                ? new AttributeValue { S = post.OtpExpirationTime.Value.ToString("o") }
                : null,
            ["CreatedAt"] = new AttributeValue { S = post.CreatedAt.ToString("o") },
        };

        if (post.MobilNumbers is not null && post.MobilNumbers.Count > 0)
        {
            item["MobilNumbers"] = new AttributeValue { SS = post.MobilNumbers };
        }

        if (post.ImageUrls is not null && post.ImageUrls.Count > 0)
        {
            item["ImageUrls"] = new AttributeValue { SS = post.ImageUrls };
        }

        // Remove nulls (DynamoDB does not accept them)
        return item
            .Where(kvp => kvp.Value != null)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
