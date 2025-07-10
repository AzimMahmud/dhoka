using System.Text;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Domain;
using Domain.Contacts;
using Infrastructure.Database;

namespace Infrastructure.Contacts;

public class ContactRepository(IAmazonDynamoDB dynamoDb) : IContactRepository
{
    public async Task CreateAsync(Contact contact)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = contact.Id.ToString() },
            ["Name"] = new AttributeValue { S = contact.Name },
            ["Email"] = new AttributeValue { S = contact.Email },
            ["MobileNumber"] = new AttributeValue { S = contact.MobileNumber },
            ["Message"] = new AttributeValue { S = contact.Message },
            ["CreatedAt"] = new AttributeValue { S = contact.CreatedAt.ToString("o") }
        };

        var request = new PutItemRequest
        {
            TableName = Tables.Contacts,
            Item = item
        };

        await dynamoDb.PutItemAsync(request);
    }

    public async Task<ContactResponse?> GetByIdAsync(Guid id)
    {
        var request = new GetItemRequest
        {
            TableName = Tables.Contacts,
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
        return new ContactResponse
        {
            Id = Guid.Parse(item["Id"].S),
            Name = item["Name"].S,
            Email = item["Email"].S,
            MobileNumber = item["MobileNumber"].S,
            Message = item["Message"].S,
            CreatedAt = DateTime.Parse(item["CreatedAt"].S)
        };
    }

    public async Task<PagedResult<ContactResponse>> GetAllContactAsync(int pageSize, string? paginationToken)
    {
        // Validate page size
        if (pageSize <= 0)
        {
            throw new ArgumentException("Page size must be greater than 0.", nameof(pageSize));
        }

        // Decode and validate pagination token
        Dictionary<string, AttributeValue>? exclusiveStartKey = null;
        if (!string.IsNullOrWhiteSpace(paginationToken))
        {
            try
            {
                byte[] decoded = Convert.FromBase64String(paginationToken);
                string json = Encoding.UTF8.GetString(decoded);
                exclusiveStartKey = JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(
                    json, new JsonSerializerOptions { Converters = { new AttributeValueConverter() } });
                if (exclusiveStartKey != null && !IsValidKey(exclusiveStartKey))
                {
                    Console.WriteLine("Invalid pagination token: key values are not properly formatted.");
                    exclusiveStartKey = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to decode pagination token: {ex.Message}");
                exclusiveStartKey = null;
            }
        }

        try
        {
            // Perform an unfiltered scan
            var scanReq = new ScanRequest
            {
                TableName = Tables.Contacts,
                Limit = pageSize,
                ExclusiveStartKey = exclusiveStartKey
            };

            ScanResponse? scanResp = await dynamoDb.ScanAsync(scanReq);
            var scanItems = scanResp.Items.Select(item => new ContactResponse
            {
                Id = Guid.TryParse(item.GetValueOrDefault("Id")?.S, out Guid id) ? id : Guid.Empty,
                Name = item.TryGetValue("Name", out AttributeValue? name) ? name.S : null,
                Email = item.TryGetValue("Email", out AttributeValue? email) ? email.S : null,
                MobileNumber = item.TryGetValue("MobileNumber", out AttributeValue? mobile) ? mobile.S : null,
                CreatedAt = item.TryGetValue("CreatedAt", out AttributeValue? createdAt) &&
                            DateTime.TryParse(createdAt.S, out DateTime dt)
                    ? dt
                    : DateTime.MinValue
            }).ToList();

            string? nextToken = null;
            if (scanResp.LastEvaluatedKey != null && scanResp.LastEvaluatedKey.Count > 0 && scanItems.Count > 0)
            {
                nextToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                    JsonSerializer.Serialize(scanResp.LastEvaluatedKey,
                        new JsonSerializerOptions { Converters = { new AttributeValueConverter() } })));
            }

            return new PagedResult<ContactResponse>
            {
                Items = scanItems,
                NextPageToken = nextToken,
                HasMore = scanResp.LastEvaluatedKey?.Count > 0 && scanItems.Count > 0
            };
        }
        catch (AmazonDynamoDBException ex)
        {
            Console.WriteLine($"DynamoDB error accessing table {Tables.Users}: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error accessing table {Tables.Users}: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var request = new DeleteItemRequest
        {
            TableName = Tables.Contacts,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id.ToString() }
            }
        };

        await dynamoDb.DeleteItemAsync(request);
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

    // Helper method to validate the key
    private bool IsValidKey(Dictionary<string, AttributeValue> key)
    {
        if (key == null || key.Count == 0)
        {
            return true; // Null or empty is valid for DynamoDB
        }

        foreach (KeyValuePair<string, AttributeValue> kvp in key)
        {
            AttributeValue? value = kvp.Value;
            if (value == null ||
                string.IsNullOrEmpty(value.S) &&
                string.IsNullOrEmpty(value.N) &&
                value.BOOL == null &&
                !value.NULL &&
                value.IsLSet == false &&
                value.IsMSet == false)
            {
                return false;
            }
        }

        return true;
    }
}
