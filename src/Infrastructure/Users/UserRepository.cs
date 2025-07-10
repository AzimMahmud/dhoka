using System.Text;
using Amazon.DynamoDBv2.Model;
using Domain;
using Domain.Users;
using Infrastructure.Database;
using Amazon.DynamoDBv2;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Users;

public class UserRepository : IUserRepository
{
    private readonly IAmazonDynamoDB _dynamoDB;
    private readonly ILogger<UserRepository> _logger;


    public UserRepository(IAmazonDynamoDB dynamoDB, ILogger<UserRepository> logger)
    {
        _dynamoDB = dynamoDB ?? throw new ArgumentNullException(nameof(dynamoDB));
        _logger = logger;
    }

    public async Task<User> GetByIdAsync(Guid id)
    {
        var request = new GetItemRequest
        {
            TableName = Tables.Users,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id.ToString() }
            }
        };

        try
        {
            GetItemResponse response = await _dynamoDB.GetItemAsync(request);
            if (response.Item == null || response.Item.Count == 0)
            {
                return null;
            }

            return MapFromDynamoDb(response.Item);
        }
        catch (AmazonDynamoDBException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve post with ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));
        }

        var request = new QueryRequest
        {
            TableName = Tables.Users,
            IndexName = "EmailLowerIndex",
            KeyConditionExpression = "#emailLower = :emailLower",
            ExpressionAttributeNames = new Dictionary<string, string> { { "#emailLower", "EmailLower" } },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":emailLower", new AttributeValue { S = email.ToLower() } }
            },
            Limit = 1
        };

        try
        {
            QueryResponse? response = await _dynamoDB.QueryAsync(request);
            if (response.Items == null || response.Items.Count == 0)
            {
                return null;
            }

            return MapFromDynamoDb(response.Items[0]);
        }
        catch (AmazonDynamoDBException ex)
        {
            if (ex.Message.Contains("not found") || ex.Message.Contains("key schema"))
            {
                throw new InvalidOperationException(
                    "EmailLowerIndex GSI is missing or misconfigured. Ensure the GSI exists with EmailLower as the partition key.",
                    ex);
            }

            throw new InvalidOperationException($"Failed to retrieve user with email {email}: {ex.Message}", ex);
        }
    }

    public async Task<PagedResult<UsersResponse>> GetAllAsync(int pageSize, string? paginationToken, string statusFilter)
    {
        // 1) Decode pagination token
        Dictionary<string, AttributeValue>? exclusiveStartKey = null;
        if (!string.IsNullOrWhiteSpace(paginationToken))
        {
            try
            {
                byte[] decoded = Convert.FromBase64String(paginationToken);
                string json = Encoding.UTF8.GetString(decoded);
                exclusiveStartKey = JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(
                    json,
                    new JsonSerializerOptions { Converters = { new AttributeValueConverter() } }
                );
            }
            catch
            {
                exclusiveStartKey = null;
            }
        }

        // If a statusFilter is provided, do a Query on the GSI
        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            var qr = new QueryRequest
            {
                TableName = Tables.Users,
                IndexName = "CreatedAtIndex", // your GSI
                KeyConditionExpression = "#st = :statusVal",
                ExpressionAttributeNames = new Dictionary<string, string> { { "#st", "Status" } },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":statusVal", new AttributeValue { S = statusFilter } }
                },
                ScanIndexForward = false, // newest first
                Limit = pageSize,
                ExclusiveStartKey = exclusiveStartKey
            };

            QueryResponse? resp = await _dynamoDB.QueryAsync(qr);

            var items = resp.Items.Select(MapToUserResponse).ToList();

            bool hasMore = resp.LastEvaluatedKey?.Count > 0;
            string? nextToken = null;
            if (hasMore)
            {
                string tokJson = JsonSerializer.Serialize(resp.LastEvaluatedKey, new JsonSerializerOptions
                {
                    Converters = { new AttributeValueConverter() }
                });
                nextToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokJson));
            }

            return new PagedResult<UsersResponse>
            {
                Items = items,
                NextPageToken = nextToken,
                HasMore = hasMore
            };
        }

        return new ();
    }


    public async Task CreateAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var request = new PutItemRequest
        {
            TableName = Tables.Users,
            Item = MapToDynamoDb(user),
            ConditionExpression = "attribute_not_exists(Id)"
        };

        try
        {
            await _dynamoDB.PutItemAsync(request);
        }
        catch (AmazonDynamoDBException ex)
        {
            throw new InvalidOperationException($"Failed to create user with ID {user.Id}: {ex.Message}", ex);
        }
    }

    public async Task UpdateAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var request = new PutItemRequest
        {
            TableName = Tables.Users,
            Item = MapToDynamoDb(user)
        };

        try
        {
            await _dynamoDB.PutItemAsync(request);
        }
        catch (AmazonDynamoDBException ex)
        {
            throw new InvalidOperationException($"Failed to update user with ID {user.Id}: {ex.Message}", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var request = new DeleteItemRequest
        {
            TableName = Tables.Users,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id.ToString() }
            }
        };

        try
        {
            await _dynamoDB.DeleteItemAsync(request);
        }
        catch (AmazonDynamoDBException ex)
        {
            throw new InvalidOperationException($"Failed to delete user with ID {id}: {ex.Message}", ex);
        }
    }

    private static User MapFromDynamoDb(Dictionary<string, AttributeValue> item)
    {
        return new User
        {
            Id = item.TryGetValue("Id", out AttributeValue? id) && Guid.TryParse(id.S, out Guid guid)
                ? guid
                : Guid.Empty,
            Email = item.GetValueOrDefault("Email")?.S,
            FirstName = item.GetValueOrDefault("FirstName")?.S,
            LastName = item.GetValueOrDefault("LastName")?.S,
            PasswordHash = item.GetValueOrDefault("PasswordHash")?.S,
            EmailVerified = item.TryGetValue("EmailVerified", out AttributeValue? ev) ? ev.BOOL : false,
            Role  = item.GetValueOrDefault("Role")?.S,
            Status = item.GetValueOrDefault("Status")?.S,
            CreatedAt = item.TryGetValue("CreatedAt", out AttributeValue? ca) ? DateTime.Parse(ca.S) : DateTime.MinValue
        };
    }

    private static Dictionary<string, AttributeValue> MapToDynamoDb(User user)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = user.Id.ToString() },
            ["Email"] = new AttributeValue { S = user.Email ?? string.Empty },
            ["EmailLower"] = new AttributeValue { S = user.Email?.ToLower() ?? string.Empty },
            ["FirstName"] = new AttributeValue { S = user.FirstName ?? string.Empty },
            ["LastName"] = new AttributeValue { S = user.LastName ?? string.Empty },
            ["PasswordHash"] = new AttributeValue { S = user.PasswordHash ?? string.Empty },
            ["EmailVerified"] = new AttributeValue { BOOL = user.EmailVerified },
            ["Status"] = new AttributeValue { S = user.Status ?? string.Empty },
            ["Role"] = new AttributeValue { S = user.Role ?? string.Empty  },
            ["CreatedAt"] = new AttributeValue { S = user.CreatedAt.ToString("o") }
       
        };

        return item.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private static UsersResponse MapToUserResponse(Dictionary<string, AttributeValue> item)
    {
        return new UsersResponse
        {
            Id = item.TryGetValue("Id", out AttributeValue? id) && Guid.TryParse(id.S, out Guid guid)
                ? guid
                : Guid.Empty,
            Email = item.GetValueOrDefault("Email")?.S ?? string.Empty,
            FirstName = item.GetValueOrDefault("FirstName")?.S ?? string.Empty,
            LastName = item.GetValueOrDefault("LastName")?.S ?? string.Empty,
            Status = item.GetValueOrDefault("Status")?.S ?? string.Empty,
            Role = item.GetValueOrDefault("Role")?.S ?? string.Empty,
        };
    }

    private Dictionary<string, AttributeValue> DecodePaginationToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        byte[] decoded = Convert.FromBase64String(token);
        string json = Encoding.UTF8.GetString(decoded);
        return JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(
            json, new JsonSerializerOptions { Converters = { new AttributeValueConverter() } });
    }

    private string EncodePaginationToken(Dictionary<string, AttributeValue> key)
    {
        string json = JsonSerializer.Serialize(
            key, new JsonSerializerOptions { Converters = { new AttributeValueConverter() } });
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }
   
}
