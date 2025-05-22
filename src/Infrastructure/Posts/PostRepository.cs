using System.Text;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Domain;
using Domain.Posts;
using OpenSearch.Client;

namespace Infrastructure.Posts;

public class PostRepository : IPostRepository
{
    private readonly IAmazonDynamoDB _dynamoDB;
    private readonly IOpenSearchClient _openSearchClient;

    private readonly string TableName = "Posts";
    private readonly string IndexName = "dhoka-data-index"; // OpenSearch index name

    public PostRepository(IAmazonDynamoDB dynamoDB, IOpenSearchClient openSearchClient)
    {
        _dynamoDB = dynamoDB;
        _openSearchClient = openSearchClient;
    }

    public async Task<Post> GetByIdAsync(Guid id)
    {
        var request = new GetItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id.ToString() } // Adjust based on schema
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

    public async Task<PagedResult<PostsResponse>> GetAllAsync(
        int pageSize,
        string? paginationToken,
        string? statusFilter = null)
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
                TableName = "Posts",
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

            var items = resp.Items.Select(MapToPostResponse).ToList();

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

            return new PagedResult<PostsResponse>
            {
                Items = items,
                NextPageToken = nextToken,
                HasMore = hasMore
            };
        }

        // Otherwise, do an unfiltered Scan
        var scanReq = new ScanRequest
        {
            TableName = "Posts",
            Limit = pageSize,
            ExclusiveStartKey = exclusiveStartKey
        };
        ScanResponse? scanResp = await _dynamoDB.ScanAsync(scanReq);

        var scanItems = scanResp.Items.Select(MapToPostResponse).ToList();

        bool scanHasMore = scanResp.LastEvaluatedKey?.Count > 0;
        string? scanNext = null;
        if (scanHasMore)
        {
            string tokJson = JsonSerializer.Serialize(scanResp.LastEvaluatedKey, new JsonSerializerOptions
            {
                Converters = { new AttributeValueConverter() }
            });
            scanNext = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokJson));
        }

        return new PagedResult<PostsResponse>
        {
            Items = scanItems,
            NextPageToken = scanNext,
            HasMore = scanHasMore
        };
    }

    public async Task<List<PostsResponse>> GetRecentPostsAsync()
    {
        var request = new QueryRequest
        {
            TableName = "Posts",
            IndexName = "CreatedAtIndex", // Ensure this GSI exists and is configured properly
            KeyConditionExpression = "#st = :statusVal",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#st", "Status" } // Alias for reserved keyword
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":statusVal", new AttributeValue { S = nameof(Status.Approved) } }
            },
            ScanIndexForward = false, // DESC order for recent posts
            Limit = 5
        };

        QueryResponse? response = await _dynamoDB.QueryAsync(request);

        var items = response.Items.Select(item => new PostsResponse
        {
            Id = Guid.Parse(item["Id"].S),
            Title = item.GetValueOrDefault("Title")?.S,
            TransactionMode = item.GetValueOrDefault("TransactionMode")?.S,
            PaymentType = item.GetValueOrDefault("PaymentType")?.S,
            Description = item.GetValueOrDefault("Description")?.S,
            MobilNumbers = item.TryGetValue("MobilNumbers", out AttributeValue? mn)
                ? mn.L.Select(x => x.S).ToList()
                : [],
            Amount = item.TryGetValue("Amount", out AttributeValue? amt) ? decimal.Parse(amt.N) : (decimal?)null,
            Status = item.GetValueOrDefault("Status")?.S ?? string.Empty,
            CreatedAt = item.TryGetValue("CreatedAt", out AttributeValue? ca) ? DateTime.Parse(ca.S) : DateTime.MinValue
        }).ToList();

        return items;
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


        if (post.IsApproved)
        {
            IndexResponse? response = await _openSearchClient.IndexAsync(post, i => i
                    .Index("dhoka-data-index")
                    .Id(post.Id) // optional, uses product.Id as document ID
            );

            if (!response.IsValid)
            {
                // handle error
                throw new Exception($"OpenSearch indexing failed: {response.OriginalException.Message}");
            }
        }
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

    public async Task<PagedSearchResult<PostsResponse>> SearchAsync(PostSearchRequest request)
    {
        int currentPage = request.CurrentPage < 1 ? 1 : request.CurrentPage;
        int pageSize = request.PageSize < 1 ? 10 : request.PageSize;
        int from = (currentPage - 1) * pageSize;

        SearchDescriptor<Post>? searchDescriptor = new SearchDescriptor<Post>()
            .From(from)
            .Size(pageSize)
            .Scroll("1m")
            .Query(q =>
            {
                var mustQueryContainer = new List<QueryContainer>();
                var filterQueryContainer = new List<QueryContainer>();


                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    mustQueryContainer.Add(q.MultiMatch(m => m
                            .Fields(f => f
                                .Field(p => p.Title)
                                .Field(p => p.Description)
                                .Field(p => p.MobilNumbers)
                            )
                            .Query(request.SearchTerm)
                            .Type(TextQueryType.MostFields) // Tries to match the best field
                            .Fuzziness(Fuzziness.Auto) // Enables fuzzy matching
                            .Operator(Operator.Or) // All terms must match (use OR if you prefer leniency)
                            .MinimumShouldMatch("75%") // Optional: only 75% of terms need to match
                    ));
                }

                filterQueryContainer.Add(q.Match(m => m
                    .Field(p => p.Status)
                    .Query(nameof(Status.Approved))));

                return q.Bool(b => b
                        .Must(mustQueryContainer.ToArray()) // Scoring conditions
                        .Filter(filterQueryContainer.ToArray()) // Non-scoring conditions
                );
            });


        ISearchResponse<PostsResponse>? response = await _openSearchClient.SearchAsync<PostsResponse>(searchDescriptor);

        long totalCount = response.Total;
        return new PagedSearchResult<PostsResponse>
        {
            Items = response.Documents,
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<List<string?>> AutocompleteTitlesAsync(AutocompleteRequest request)
    {
        // Use MatchPhrasePrefixQuery for case-insensitive prefix matching
        var boolQuery = new BoolQuery
        {
            Must = new QueryContainer[]
            {
                new MatchPhrasePrefixQuery
                {
                    Field = "title", // Use analyzed 'title' field instead of 'title.keyword'
                    Query = request.Prefix, // No need to force ToLower() here
                    MaxExpansions = 50 // Limit expansions for performance
                }
            },
            Filter = new QueryContainer[]
            {
                new TermQuery
                {
                    Field = "status.keyword",
                    Value = nameof(Status.Approved)
                }
            }
        };

        var searchRequest = new SearchRequest(IndexName)
        {
            Size = 10, // Increased cap to 10 for more flexibility
            Query = boolQuery,
            Source = new SourceFilter
            {
                Includes = new[] { "title" }
            }
        };

        try
        {
            ISearchResponse<dynamic>? response = await _openSearchClient.SearchAsync<dynamic>(searchRequest);

            if (!response.IsValid)
            {
                throw new InvalidOperationException(
                    $"OpenSearch autocomplete query failed: {response.ServerError?.Error.Reason ?? "Unknown error"}"
                );
            }

            return response.Hits
                .Select(hit => (string?)hit.Source.title)
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .ToList();
        }
        catch (Exception ex)
        {
            throw; // Re-throw to allow caller to handle
        }
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
            ["CreatedAt"] = new AttributeValue
                { S = post.CreatedAt.HasValue ? post.CreatedAt.Value.ToString("o") : string.Empty },
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


    private PostsResponse MapToPostResponse(Dictionary<string, AttributeValue> item)
    {
        return new PostsResponse
        {
            Id = Guid.Parse(item["Id"].S),
            Title = item.GetValueOrDefault("Title")?.S,
            TransactionMode = item.GetValueOrDefault("TransactionMode")?.S,
            PaymentType = item.GetValueOrDefault("PaymentType")?.S,
            Description = item.GetValueOrDefault("Description")?.S,
            MobilNumbers = item.TryGetValue("MobilNumbers", out AttributeValue? mn)
                ? mn.L.Select(x => x.S).ToList()
                : new List<string>(),
            Amount = item.TryGetValue("Amount", out AttributeValue? amt)
                ? decimal.Parse(amt.N)
                : (decimal?)null,
            Status = item.GetValueOrDefault("Status")?.S ?? string.Empty,
            CreatedAt = item.TryGetValue("CreatedAt", out AttributeValue? ca)
                ? DateTime.Parse(ca.S)
                : DateTime.MinValue,
        };
    }

    private Dictionary<string, AttributeValue>? DecodePaginationToken(string? paginationToken)
    {
        if (string.IsNullOrWhiteSpace(paginationToken))
        {
            return null;
        }

        string json = Encoding.UTF8.GetString(Convert.FromBase64String(paginationToken));
        return JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(json, new JsonSerializerOptions
        {
            Converters = { new AttributeValueConverter() }
        });
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
