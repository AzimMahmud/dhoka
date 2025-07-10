using System.Text;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Domain;
using Domain.Posts;
using Infrastructure.Database;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;
using OpenSearch.Net;
using Polly;
using SharedKernel;
using Status = Domain.Posts.Status;

namespace Infrastructure.Posts;

public partial class PostRepository : IPostRepository
{
    private readonly IImageService _imageService;
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly IOpenSearchClient _openSearchClient;
    private readonly ILogger<PostRepository> _logger;
    private const string IndexName = "dhoka-data-index";
    private readonly Polly.Retry.AsyncRetryPolicy _retryPolicy;
    
    private const string StatusAttribute = "Status";
    private const string InitStatusValue = "Init";
    private const string PartitionKey = "Id";
    private const string ImageUrlsAttr = "ImageUrls";

    public PostRepository(
        IImageService imageService,
        IAmazonDynamoDB dynamoDb,
        IOpenSearchClient openSearchClient,
        ILogger<PostRepository> logger)
    {
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        _dynamoDb = dynamoDb ?? throw new ArgumentNullException(nameof(dynamoDb));
        _openSearchClient = openSearchClient ?? throw new ArgumentNullException(nameof(openSearchClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _retryPolicy = Policy
            .Handle<Exception>(ex =>
                ex.Message.Contains("502") ||
                ex.Message.Contains("503") ||
                ex is System.Net.Http.HttpRequestException ||
                ex is OpenSearchClientException)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                    _logger.LogWarning(
                        $"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}")
            );
    }

    public async Task<Post> GetByIdAsync(Guid id)
    {
        _logger.LogInformation($"Retrieving post {id} from DynamoDB.");
        var request = new GetItemRequest
        {
            TableName = Tables.Posts,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id.ToString() }
            }
        };

        try
        {
            GetItemResponse? response = await _dynamoDb.GetItemAsync(request);
            if (response.Item == null || response.Item.Count == 0)
            {
                _logger.LogInformation($"Post {id} not found in DynamoDB.");
                return null;
            }

            return MapFromDynamoDb(response.Item);
        }
        catch (AmazonDynamoDBException ex)
        {
            _logger.LogError(ex, $"Failed to retrieve post {id} from DynamoDB.");
            throw new InvalidOperationException($"Failed to retrieve post {id}: {ex.Message}", ex);
        }
    }

    public async Task<PagedResult<PostsResponse>> GetAllAsync(
        int pageSize,
        string? paginationToken,
        string? statusFilter = null)
    {
        _logger.LogInformation(
            $"Fetching posts with pageSize={pageSize}, token={paginationToken}, statusFilter={statusFilter}.");

        Dictionary<string, AttributeValue>? exclusiveStartKey = null;
        if (!string.IsNullOrWhiteSpace(paginationToken))
        {
            try
            {
                byte[] decoded = Convert.FromBase64String(paginationToken);
                string json = Encoding.UTF8.GetString(decoded);
                exclusiveStartKey = JsonSerializer.Deserialize<Dictionary<string, AttributeValue>>(
                    json, new JsonSerializerOptions { Converters = { new AttributeValueConverter() } });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to decode pagination token: {paginationToken}. Proceeding without it.");
                exclusiveStartKey = null;
            }
        }

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            var queryRequest = new QueryRequest
            {
                TableName = Tables.Posts,
                IndexName = "CreatedAtIndex",
                KeyConditionExpression = "#st = :statusVal",
                ExpressionAttributeNames = new Dictionary<string, string> { { "#st", StatusAttribute } },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":statusVal", new AttributeValue { S = statusFilter } }
                },
                ScanIndexForward = false,
                Limit = pageSize,
                ExclusiveStartKey = exclusiveStartKey
            };

            QueryResponse? response = await _dynamoDb.QueryAsync(queryRequest);
            var items = response.Items.Select(MapToPostResponse).ToList();
            bool hasMore = response.LastEvaluatedKey?.Count > 0;
            string? nextToken = hasMore ? EncodePaginationToken(response.LastEvaluatedKey) : null;

            return new PagedResult<PostsResponse>
            {
                Items = items,
                NextPageToken = nextToken,
                HasMore = hasMore
            };
        }

        var scanRequest = new ScanRequest
        {
            TableName = Tables.Posts,
            Limit = pageSize,
            ExclusiveStartKey = exclusiveStartKey
        };
        ScanResponse? scanResponse = await _dynamoDb.ScanAsync(scanRequest);
        var scanItems = scanResponse.Items.Select(MapToPostResponse).ToList();
        bool scanHasMore = scanResponse.LastEvaluatedKey?.Count > 0;
        string? scanNext = scanHasMore ? EncodePaginationToken(scanResponse.LastEvaluatedKey) : null;

        return new PagedResult<PostsResponse>
        {
            Items = scanItems,
            NextPageToken = scanNext,
            HasMore = scanHasMore
        };
    }

    public async Task<List<PostsResponse>> GetRecentPostsAsync()
    {
        _logger.LogInformation("Fetching 5 most recent approved public posts.");
        var request = new QueryRequest
        {
            TableName = Tables.Posts,
            IndexName = "CreatedAtIndex",
            KeyConditionExpression = "#st = :statusVal",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#st", StatusAttribute },
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":statusVal", new AttributeValue { S = nameof(Status.Approved) } },
            },
            ScanIndexForward = false,
            Limit = 5
        };

        QueryResponse? response = await _dynamoDb.QueryAsync(request);
        return response.Items.Select(MapToPostResponse).ToList();
    }

    public async Task CreateAsync(Post post)
    {
        ValidatePost(post, isCreate: true);
        _logger.LogInformation($"Creating post {post.Id} in DynamoDB.");
        var request = new PutItemRequest
        {
            TableName = Tables.Posts,
            Item = MapToDynamoDb(post)
        };

        try
        {
            await _dynamoDb.PutItemAsync(request);
            _logger.LogInformation($"Successfully created post {post.Id} in DynamoDB.");
        }
        catch (AmazonDynamoDBException ex)
        {
            _logger.LogError(ex, $"Failed to create post {post.Id} in DynamoDB.");
            throw new InvalidOperationException($"Failed to create post {post.Id}: {ex.Message}", ex);
        }
    }

    public async Task UpdateAsync(Post post)
    {
        ValidatePost(post, isCreate: false);
        _logger.LogInformation($"Updating post {post.Id} in DynamoDB.");

        if (post.IsApproved)
        {
            _logger.LogInformation($"Post {post.Id} is approved, indexing in OpenSearch.");
            var indexedPost = new IndexedPost
            {
                Id = post.Id,
                ScamType = post.ScamType,
                Title = post.Title,
                Description = post.Description,
                PaymentType = post.PaymentType,
                ScamDateTime = post.ScamDateTime,
                MobileNumbers = post.MobileNumbers ?? new List<string>(),
                PaymentDetails = post.PaymentDetails,
                Amount = post.Amount,
                CreatedAt = post.CreatedAt,
            };

            IndexResponse? opensearchResponse = null;
            try
            {
                opensearchResponse = await _retryPolicy.ExecuteAsync(async () =>
                    await _openSearchClient.IndexAsync(indexedPost, i => i
                        .Index(IndexName)
                        .Id(post.Id.ToString())
                    ));

                if (!opensearchResponse.IsValid)
                {
                    string errMsg = opensearchResponse.OriginalException?.Message ?? "Unknown OpenSearch error";
                    _logger.LogError($"OpenSearch indexing failed for post {post.Id}: {errMsg}");
                    throw new InvalidOperationException($"OpenSearch indexing failed: {errMsg}");
                }

                _logger.LogInformation($"Successfully indexed post {post.Id} in OpenSearch.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to index post {post.Id} in OpenSearch.");
                throw new InvalidOperationException($"Failed to index post {post.Id}: {ex.Message}", ex);
            }
        }

        var request = new PutItemRequest
        {
            TableName = Tables.Posts,
            Item = MapToDynamoDb(post)
        };

        try
        {
            await _dynamoDb.PutItemAsync(request);
            _logger.LogInformation($"Successfully updated post {post.Id} in DynamoDB.");
        }
        catch (AmazonDynamoDBException dbEx)
        {
            _logger.LogError(dbEx, $"DynamoDB update failed for post {post.Id}. Attempting cleanup.");
            if (post.IsApproved)
            {
                try
                {
                    DeleteResponse? deleteResponse = await _retryPolicy.ExecuteAsync(async () =>
                        await _openSearchClient.DeleteAsync<IndexedPost>(
                            post.Id.ToString(),
                            d => d.Index(IndexName)
                        ));

                    if (!deleteResponse.IsValid)
                    {
                        _logger.LogWarning(
                            $"Compensating delete failed for post {post.Id} in OpenSearch: {deleteResponse.ServerError}");
                    }
                    else
                    {
                        _logger.LogInformation($"Compensating delete succeeded for post {post.Id} in OpenSearch.");
                    }
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx,
                        $"Failed to delete post {post.Id} from OpenSearch during cleanup. System may be inconsistent.");
                    // Note: Consider queuing for later reconciliation in a production system.
                }
            }

            throw new InvalidOperationException(
                $"Failed to update post {post.Id} in DynamoDB after OpenSearch indexing. " +
                $"Cleanup attempted. Original error: {dbEx.Message}",
                dbEx);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation($"Deleting post {id} from DynamoDB.");
        var request = new DeleteItemRequest
        {
            TableName = Tables.Posts,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id.ToString() }
            }
        };

        try
        {
            DeleteItemResponse? response = await _dynamoDb.DeleteItemAsync(request);
            _logger.LogInformation($"Successfully deleted post {id} from DynamoDB.");
        }
        catch (AmazonDynamoDBException ex)
        {
            _logger.LogError(ex, $"Failed to delete post {id} from DynamoDB.");
            throw new InvalidOperationException($"Failed to delete post {id}: {ex.Message}", ex);
        }
    }

    public async Task EnsurePostIndexedAsync(Guid id)
    {
        _logger.LogInformation($"Ensuring post {id} is indexed in OpenSearch.");
        GetResponse<IndexedPost>? getResponse = await _retryPolicy.ExecuteAsync(async () =>
            await _openSearchClient.GetAsync<IndexedPost>(id.ToString(), g => g.Index(IndexName)));

        if (getResponse.IsValid && getResponse.Source != null)
        {
            _logger.LogInformation($"Post {id} already indexed in OpenSearch.");
            return;
        }

        var dynamoRequest = new GetItemRequest
        {
            TableName = Tables.Posts,
            Key = new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = id.ToString() }
            }
        };

        Post post;
        try
        {
            GetItemResponse? dynamoResponse = await _dynamoDb.GetItemAsync(dynamoRequest);
            if (dynamoResponse.Item == null || dynamoResponse.Item.Count == 0)
            {
                _logger.LogWarning($"Post {id} not found in DynamoDB for indexing.");
                return;
            }

            post = MapFromDynamoDb(dynamoResponse.Item);
        }
        catch (AmazonDynamoDBException ex)
        {
            _logger.LogError(ex, $"Failed to fetch post {id} from DynamoDB for indexing.");
            throw new InvalidOperationException($"Failed to fetch post {id}: {ex.Message}", ex);
        }

        if (!post.IsApproved)
        {
            _logger.LogInformation($"Post {id} is not approved, skipping indexing.");
            return;
        }

        var indexedPost = new IndexedPost
        {
            Id = post.Id,
            ScamType = post.ScamType,
            Title = post.Title,
            Description = post.Description,
            PaymentType = post.PaymentType,
            ScamDateTime = post.ScamDateTime,
            MobileNumbers = post.MobileNumbers ?? new List<string>(),
            PaymentDetails = post.PaymentDetails,
            Amount = post.Amount,
            CreatedAt = post.CreatedAt
        };

        IndexResponse? indexResponse = await _retryPolicy.ExecuteAsync(async () =>
            await _openSearchClient.IndexAsync(indexedPost, i => i
                .Index(IndexName)
                .Id(post.Id.ToString())
            ));

        if (!indexResponse.IsValid)
        {
            string errorMsg = indexResponse.OriginalException?.Message ?? "Unknown error";
            _logger.LogError($"Failed to index post {id} in OpenSearch: {errorMsg}");
            throw new InvalidOperationException($"Failed to index post {id}: {errorMsg}");
        }

        _logger.LogInformation($"Successfully indexed post {id} in OpenSearch.");
    }

    public async Task<PagedSearchResult<PostsResponse>> SearchAsync(PostSearchRequest request)
    {
        _logger.LogInformation($"Searching posts with term: {request.SearchTerm}, page: {request.CurrentPage}");
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return new PagedSearchResult<PostsResponse>
            {
                Items = new List<PostsResponse>(),
                CurrentPage = request.CurrentPage < 1 ? 1 : request.CurrentPage,
                PageSize = request.PageSize < 1 ? 10 : request.PageSize,
                TotalCount = 0
            };
        }

        int currentPage = request.CurrentPage < 1 ? 1 : request.CurrentPage;
        int pageSize = request.PageSize < 1 ? 10 : request.PageSize;
        int from = (currentPage - 1) * pageSize;

        SearchDescriptor<PostsResponse>? searchDescriptor = new SearchDescriptor<PostsResponse>()
            .Index(IndexName)
            .From(from)
            .Size(pageSize)
            .Query(q => q
                .MultiMatch(m => m
                    .Fields(f => f
                        .Field(p => p.Title)
                        .Field(p => p.Description)
                        .Field(p => p.ScamType)
                        .Field(p => p.PaymentType)
                    )
                    .Query(request.SearchTerm)
                    .Type(TextQueryType.MostFields)
                    .Fuzziness(Fuzziness.Auto)
                    .Operator(Operator.Or)
                    .MinimumShouldMatch("75%")
                )
            );

        ISearchResponse<PostsResponse>? response = await _openSearchClient.SearchAsync<PostsResponse>(searchDescriptor);
        if (!response.IsValid)
        {
            _logger.LogError($"Search failed: {response.ServerError}");
            throw new InvalidOperationException($"Search failed: {response.ServerError}");
        }

        return new PagedSearchResult<PostsResponse>
        {
            Items = response.Documents,
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalCount = response.Total
        };
    }

    public async Task<List<string>> AutocompleteTitlesAsync(AutocompleteRequest request)
    {
        _logger.LogInformation($"Autocompleting titles with prefix: {request.Prefix}");
        var boolQuery = new BoolQuery
        {
            Must = new QueryContainer[]
            {
                new MatchPhrasePrefixQuery
                {
                    Field = "Title",
                    Query = request.Prefix,
                    MaxExpansions = 50
                }
            }
        };

        var searchRequest = new SearchRequest(IndexName)
        {
            Size = 10,
            Query = boolQuery,
            Source = new SourceFilter { Includes = new[] { "Title" } },
            Sort = new List<ISort>
            {
                new FieldSort { Field = "Title.keyword", Order = SortOrder.Ascending }
            }
        };

        ISearchResponse<TitleResponse>? response = await _openSearchClient.SearchAsync<TitleResponse>(searchRequest);
        if (!response.IsValid)
        {
            _logger.LogError($"Autocomplete failed: {response.ServerError}");
            throw new InvalidOperationException($"Autocomplete failed: {response.ServerError?.Error.Reason}");
        }

        return response.Hits
            .Select(hit => hit.Source.Title)
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct()
            .ToList();
    }

    #region Private Helper Methods

    private static Post MapFromDynamoDb(Dictionary<string, AttributeValue> item)
    {
        return new Post
        {
            Id = Guid.Parse(item["Id"].S),
            ScamType = item.GetValueOrDefault("ScamType")?.S,
            Title = item.GetValueOrDefault("Title")?.S,
            PaymentType = item.GetValueOrDefault("PaymentType")?.S,
            Description = item.GetValueOrDefault("Description")?.S,
            MobileNumbers = item.GetValueOrDefault("MobileNumbers")?.SS?.ToList() ?? new List<string>(),
            Amount = item.TryGetValue("Amount", out AttributeValue? amtAttr) ? decimal.Parse(amtAttr.N) : null,
            PaymentDetails = item.GetValueOrDefault("PaymentDetails")?.S,
            ScamDateTime = item.TryGetValue("ScamDateTime", out AttributeValue? dt) ? DateTime.Parse(dt.S) : null,
            AnonymityPreference = item.GetValueOrDefault("AnonymityPreference")?.S,
            Name = item.GetValueOrDefault("Name")?.S,
            Otp = item.TryGetValue("Otp", out AttributeValue? otpAttr) ? int.Parse(otpAttr.N) : int.MinValue,
            Status = item.GetValueOrDefault("Status")?.S ?? string.Empty,
            ContactNumber = item.GetValueOrDefault("ContactNumber")?.S,
            CreatedAt = item.TryGetValue("CreatedAt", out AttributeValue? ca) ? DateTime.Parse(ca.S) : null,
            ImageUrls = item.GetValueOrDefault("ImageUrls")?.SS?.ToList() ?? new List<string>()
        };
    }

    private static Dictionary<string, AttributeValue> MapToDynamoDb(Post post)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new AttributeValue { S = post.Id.ToString() },
            ["ScamType"] = new AttributeValue { S = post.ScamType ?? string.Empty },
            ["Title"] = new AttributeValue { S = post.Title ?? string.Empty },
            ["PaymentType"] = new AttributeValue { S = post.PaymentType ?? string.Empty },
            ["Description"] = new AttributeValue { S = post.Description ?? string.Empty },
            ["Amount"] = post.Amount.HasValue ? new AttributeValue { N = post.Amount.Value.ToString() } : null,
            ["PaymentDetails"] = new AttributeValue { S = post.PaymentDetails ?? string.Empty },
            ["ScamDateTime"] = post.ScamDateTime.HasValue
                ? new AttributeValue { S = post.ScamDateTime.Value.ToString("o") }
                : null,
            ["AnonymityPreference"] = new AttributeValue { S = post.AnonymityPreference ?? string.Empty },
            ["Name"] = new AttributeValue { S = post.Name ?? string.Empty },
            ["Status"] = new AttributeValue { S = post.Status ?? string.Empty },
            ["ContactNumber"] = new AttributeValue { S = post.ContactNumber ?? string.Empty },
            ["Otp"] = new AttributeValue { N = post.Otp.ToString() },
            ["CreatedAt"] = post.CreatedAt.HasValue
                ? new AttributeValue { S = post.CreatedAt.Value.ToString("o") }
                : null
        };

        if (post.MobileNumbers?.Count > 0)
        {
            item["MobileNumbers"] = new AttributeValue { SS = post.MobileNumbers };
        }

        if (post.ImageUrls?.Count > 0)
        {
            item["ImageUrls"] = new AttributeValue { SS = post.ImageUrls };
        }

        return item.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private PostsResponse MapToPostResponse(Dictionary<string, AttributeValue> item)
    {
        return new PostsResponse
        {
            Id = Guid.Parse(item["Id"].S),
            ScamType = item.GetValueOrDefault("ScamType")?.S,
            Title = item.GetValueOrDefault("Title")?.S,
            Anonymity = item.GetValueOrDefault("AnonymityPreference")?.S,
            PaymentType = item.GetValueOrDefault("PaymentType")?.S,
            Amount = item.TryGetValue("Amount", out AttributeValue? amtAttr) ? decimal.Parse(amtAttr.N) : null,
            PaymentDetails = item.GetValueOrDefault("PaymentDetails")?.S,
            ScamDateTime = item.TryGetValue("ScamDateTime", out AttributeValue? dtAttr) ? DateTime.Parse(dtAttr.S) : null,
            Status = item.GetValueOrDefault("Status")?.S,
            CreatedAt = item.TryGetValue("CreatedAt", out AttributeValue? caAttr) ? DateTime.Parse(caAttr.S) : null
        };
    }

    private string EncodePaginationToken(Dictionary<string, AttributeValue> lastEvaluatedKey)
    {
        string tokJson = JsonSerializer.Serialize(lastEvaluatedKey, new JsonSerializerOptions
        {
            Converters = { new AttributeValueConverter() }
        });
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(tokJson));
    }

    private void ValidatePost(Post post, bool isCreate)
    {
        if (post == null)
        {
            throw new ArgumentNullException(nameof(post));
        }

        if (isCreate && post.Id == Guid.Empty)
        {
            throw new ArgumentException("Post ID cannot be empty for creation.");
        }

        if (string.IsNullOrWhiteSpace(post.Status))
        {
            throw new ArgumentException("Status is required.");
        }
    }

    #endregion
}
