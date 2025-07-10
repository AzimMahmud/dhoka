using Domain;
using Domain.Tokens;
using Infrastructure.Database;
using OpenSearch.Client;

namespace Infrastructure.Tokens;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


public class EmailVerificationTokenRepository(IAmazonDynamoDB dynamoDb) : IEmailVerificationTokenRepository
{
    private readonly IAmazonDynamoDB _dynamoDB = dynamoDb ?? throw new ArgumentNullException(nameof(dynamoDb));



    public async Task<EmailVerificationToken> GetByIdAsync(Guid id)
    {
       
        var request = new GetItemRequest
        {
            TableName = Tables.EmailVerificationTokens,
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
            
            if (!response.IsItemSet)
            {
                return null;
            }

            return MapFromDynamoDb(response.Item);
        }
        catch (AmazonDynamoDBException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve email verification token with ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<PagedResult<EmailVerificationToken>> GetByUserIdAsync(Guid userId, int pageSize, string? paginationToken)
    {
        if (pageSize <= 0)
        {
            throw new ArgumentException("PageSize must be greater than 0.", nameof(pageSize));
        }

        Dictionary<string, AttributeValue>? exclusiveStartKey = DecodePaginationToken(paginationToken);

        var request = new QueryRequest
        {
            TableName = Tables.EmailVerificationTokens,
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
            var items = response.Items.Select(MapFromDynamoDb).Where(t => t.ExpiresOnUtc > DateTime.UtcNow).ToList();

            bool hasMore = response.LastEvaluatedKey?.Count > 0;
            string? nextToken = hasMore ? EncodePaginationToken(response.LastEvaluatedKey) : null;

            return new PagedResult<EmailVerificationToken>
            {
                Items = items,
                NextPageToken = nextToken,
                HasMore = hasMore
            };
        }
        catch (AmazonDynamoDBException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve email verification tokens for user {userId}: {ex.Message}", ex);
        }
    }

    public async Task CreateAsync(EmailVerificationToken token)
    {
        if (token == null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        var request = new PutItemRequest
        {
            TableName = Tables.EmailVerificationTokens,
            Item = MapToDynamoDb(token),
            ConditionExpression = "attribute_not_exists(Id)"
        };

        try
        {
            await _dynamoDB.PutItemAsync(request);
        }
        catch (AmazonDynamoDBException ex)
        {
            throw new InvalidOperationException($"Failed to create email verification token with ID {token.Id}: {ex.Message}", ex);
        }
    }

    public async Task UpdateAsync(EmailVerificationToken token)
    {
        if (token == null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        var request = new PutItemRequest
        {
            TableName = Tables.EmailVerificationTokens,
            Item = MapToDynamoDb(token)
        };

        try
        {
            await _dynamoDB.PutItemAsync(request);
        }
        catch (AmazonDynamoDBException ex)
        {
            throw new InvalidOperationException($"Failed to update email verification token with ID {token.Id}: {ex.Message}", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var request = new DeleteItemRequest
        {
            TableName = Tables.EmailVerificationTokens,
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
            throw new InvalidOperationException($"Failed to delete email verification token with ID {id}: {ex.Message}", ex);
        }
    }

    private static EmailVerificationToken MapFromDynamoDb(Dictionary<string, AttributeValue> item)
    {
        return new EmailVerificationToken
        {
            Id = item.TryGetValue("Id", out AttributeValue? id) && Guid.TryParse(id.S, out Guid guid) ? guid : Guid.Empty,
            UserId = item.TryGetValue("UserId", out AttributeValue? userId) && Guid.TryParse(userId.S, out Guid userGuid) ? userGuid : Guid.Empty,
            CreatedOnUtc = item.TryGetValue("CreatedOnUtc", out AttributeValue? created) ? DateTime.Parse(created.S) : DateTime.MinValue,
            ExpiresOnUtc = item.TryGetValue("ExpiresOnUtc", out AttributeValue? expires) ? DateTime.Parse(expires.S) : DateTime.MinValue
        };
    }

    private static Dictionary<string, AttributeValue> MapToDynamoDb(EmailVerificationToken token)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = token.Id.ToString() },
            ["UserId"] = new AttributeValue { S = token.UserId.ToString() },
            ["CreatedOnUtc"] = new AttributeValue { S = token.CreatedOnUtc.ToString("o") },
            ["ExpiresOnUtc"] = new AttributeValue { S = token.ExpiresOnUtc.ToString("o") },
            ["ExpiresOnUtcTTL"] = new AttributeValue { N = ((DateTimeOffset)token.ExpiresOnUtc).ToUnixTimeSeconds().ToString() }
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
