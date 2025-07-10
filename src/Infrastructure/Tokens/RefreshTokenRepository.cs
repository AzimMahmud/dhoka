using Infrastructure.Database;
using Domain;
using Domain.Tokens;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Tokens;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IAmazonDynamoDB _dynamoDB;

    public RefreshTokenRepository(IAmazonDynamoDB dynamoDB)
    {
        _dynamoDB = dynamoDB ?? throw new ArgumentNullException(nameof(dynamoDB));
    }
    public async Task<RefreshToken> GetByTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }

        var request = new QueryRequest
        {
            TableName = Tables.RefreshTokens,
            IndexName = "TokenIndex",
            KeyConditionExpression = "#token = :token",
            ExpressionAttributeNames = new Dictionary<string, string> { { "#token", "Token" } },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":token", new AttributeValue { S = token } }
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
            throw new InvalidOperationException($"Failed to retrieve refresh token with token {token}: {ex.Message}", ex);
        }
    }

    public async Task<PagedResult<RefreshToken>> GetByUserIdAsync(Guid userId, int pageSize, string? paginationToken)
    {
        if (pageSize <= 0)
        {
            throw new ArgumentException("PageSize must be greater than 0.", nameof(pageSize));
        }

        Dictionary<string, AttributeValue>? exclusiveStartKey = DecodePaginationToken(paginationToken);

        var request = new QueryRequest
        {
            TableName = Tables.RefreshTokens,
            IndexName = "UserIdIndex",
            KeyConditionExpression = "#userId = :userId",
            ExpressionAttributeNames = new Dictionary<string, string> { { "#userId", "UserId" } },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":userId", new AttributeValue { S = userId.ToString() } }
            },
            Limit = pageSize,
            ExclusiveStartKey = exclusiveStartKey
        };

        try
        {
            QueryResponse? response = await _dynamoDB.QueryAsync(request);
            var items = response.Items.Select(MapFromDynamoDb).ToList();

            bool hasMore = response.LastEvaluatedKey?.Count > 0;
            string? nextToken = hasMore ? EncodePaginationToken(response.LastEvaluatedKey) : null;

            return new PagedResult<RefreshToken>
            {
                Items = items,
                NextPageToken = nextToken,
                HasMore = hasMore
            };
        }
        catch (AmazonDynamoDBException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve refresh tokens for user {userId}: {ex.Message}", ex);
        }
    }

    public async Task CreateAsync(RefreshToken refreshToken)
    {
        if (refreshToken == null)
        {
            throw new ArgumentNullException(nameof(refreshToken));
        }

        var request = new PutItemRequest
        {
            TableName = Tables.RefreshTokens,
            Item = MapToDynamoDb(refreshToken),
            ConditionExpression = "attribute_not_exists(Id)"
        };

        try
        {
            await _dynamoDB.PutItemAsync(request);
        }
        catch (AmazonDynamoDBException ex)
        {
            throw new InvalidOperationException($"Failed to create refresh token with ID {refreshToken.Id}: {ex.Message}", ex);
        }
    }

    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        if (refreshToken == null)
        {
            throw new ArgumentNullException(nameof(refreshToken));
        }

        var request = new PutItemRequest
        {
            TableName = Tables.RefreshTokens,
            Item = MapToDynamoDb(refreshToken)
        };

        try
        {
            await _dynamoDB.PutItemAsync(request);
        }
        catch (AmazonDynamoDBException ex)
        {
            throw new InvalidOperationException($"Failed to update refresh token with ID {refreshToken.Id}: {ex.Message}", ex);
        }
    }

    public async Task DeleteByUserIdAsync(Guid userId)
    {
        // Initialize variables for pagination
        Dictionary<string, AttributeValue>? exclusiveStartKey = null;
        const int pageSize = 25; // Matches DynamoDB BatchWriteItem limit

        try
        {
            do
            {
                // Query UserIdIndex to get token IDs
                var queryRequest = new QueryRequest
                {
                    TableName = Tables.RefreshTokens,
                    IndexName = "UserIdIndex",
                    KeyConditionExpression = "#userId = :userId",
                    ExpressionAttributeNames = new Dictionary<string, string> { { "#userId", "UserId" } },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":userId", new AttributeValue { S = userId.ToString() } }
                    },
                    ProjectionExpression = "Id", // Only fetch Id to minimize data transfer
                    Limit = pageSize,
                    ExclusiveStartKey = exclusiveStartKey
                };

                QueryResponse? queryResponse = await _dynamoDB.QueryAsync(queryRequest);

                // If no items, exit loop
                if (queryResponse.Items == null || queryResponse.Items.Count == 0)
                {
                    break;
                }

                // Prepare batch delete request
                var deleteRequests = queryResponse.Items.Select(item => new WriteRequest
                {
                    DeleteRequest = new DeleteRequest
                    {
                        Key = new Dictionary<string, AttributeValue>
                        {
                            ["Id"] = item["Id"]
                        }
                    }
                }).ToList();

                // Execute batch delete
                var batchRequest = new BatchWriteItemRequest
                {
                    RequestItems = new Dictionary<string, List<WriteRequest>>
                    {
                        { Tables.RefreshTokens, deleteRequests }
                    }
                };

                await _dynamoDB.BatchWriteItemAsync(batchRequest);

                // Update pagination key
                exclusiveStartKey = queryResponse.LastEvaluatedKey?.Count > 0 ? queryResponse.LastEvaluatedKey : null;

            } while (exclusiveStartKey != null);
        }
        catch (AmazonDynamoDBException ex)
        {
            throw new InvalidOperationException($"Failed to delete refresh tokens for user {userId}: {ex.Message}", ex);
        }
    }

    private static RefreshToken MapFromDynamoDb(Dictionary<string, AttributeValue> item)
    {
        return new RefreshToken
        {
            Id = item.TryGetValue("Id", out AttributeValue? id) && Guid.TryParse(id.S, out Guid guid) ? guid : Guid.Empty,
            Token = item.GetValueOrDefault("Token")?.S,
            UserId = item.TryGetValue("UserId", out AttributeValue? userId) && Guid.TryParse(userId.S, out Guid userGuid) ? userGuid : Guid.Empty,
            ExpiresOnUtc = item.TryGetValue("ExpiresOnUtc", out AttributeValue? expires) ? DateTime.Parse(expires.S) : DateTime.MinValue
        };
    }

    private static Dictionary<string, AttributeValue> MapToDynamoDb(RefreshToken refreshToken)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = refreshToken.Id.ToString() },
            ["Token"] = new AttributeValue { S = refreshToken.Token ?? string.Empty },
            ["UserId"] = new AttributeValue { S = refreshToken.UserId.ToString() },
            ["ExpiresOnUtc"] = new AttributeValue { S = refreshToken.ExpiresOnUtc.ToString("o") }
        };

        return item.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private Dictionary<string, AttributeValue>? DecodePaginationToken(string? paginationToken)
    {
        if (string.IsNullOrWhiteSpace(paginationToken))
        {
            return null;
        }

        try
        {
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(paginationToken));
            return JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(json, new JsonSerializerOptions
            {
                Converters = { new AttributeValueConverter() }
            });
        }
        catch
        {
            return null;
        }
    }

    private string EncodePaginationToken(Dictionary<string, AttributeValue> lastEvaluatedKey)
    {
        string json = JsonSerializer.Serialize(lastEvaluatedKey, new JsonSerializerOptions
        {
            Converters = { new AttributeValueConverter() }
        });
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }
}
