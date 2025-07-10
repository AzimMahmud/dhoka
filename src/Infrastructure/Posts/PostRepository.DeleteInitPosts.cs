using Infrastructure.Database;
using SharedKernel;

namespace Infrastructure.Posts;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public partial class PostRepository
{
    /// <summary>
    /// Scans all posts where Status == "init".  
    /// For each post:
    ///   1) Tries to delete all images (S3 + CloudFront) with retries.  
    ///   2) If images deleted successfully, deletes the DynamoDB record.  
    ///   3) If image deletion ultimately fails, logs & skips; we do NOT delete the DB item this run.
    /// </summary>
    public async Task DeleteAllInitPostsAndImagesAsync(CancellationToken cancellationToken = default)
    {
        var postsToDelete = new List<(Dictionary<string, AttributeValue> Key, List<string> ImageUrls)>();

        // ─── Fix the FilterExpression by aliasing “Status” as “#st” ──────────────────────────
        var scanRequest = new ScanRequest
        {
            TableName = Tables.Posts,
            ProjectionExpression = $"{PartitionKey}, {ImageUrlsAttr}",
            FilterExpression = "#st = :initVal",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                ["#st"] = StatusAttribute
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":initVal"] = new AttributeValue { S = InitStatusValue }
            }
        };

        do
        {
            ScanResponse? scanResponse = await _dynamoDb.ScanAsync(scanRequest, cancellationToken);

            foreach (Dictionary<string, AttributeValue>? item in scanResponse.Items)
            {
                var keyDict = new Dictionary<string, AttributeValue>
                {
                    [PartitionKey] = item[PartitionKey]
                };

                List<string> imageUrls =
                    item.TryGetValue(ImageUrlsAttr, out AttributeValue? imgAttr) && imgAttr.SS != null
                        ? imgAttr.SS.ToList()
                        : new List<string>();

                postsToDelete.Add((keyDict, imageUrls));
            }

            scanRequest.ExclusiveStartKey = scanResponse.LastEvaluatedKey;
        } while (scanRequest.ExclusiveStartKey != null && scanRequest.ExclusiveStartKey.Count > 0);

        _logger.LogInformation("Found {Count} posts with Status=\"init\".", postsToDelete.Count);

        // 2) Process each post one by one (so that partial failures do not block other items)
        foreach ((Dictionary<string, AttributeValue> keyDict, List<string> imageUrls) in postsToDelete)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            // a) First, try deleting images (S3 + CloudFront) with retry
            bool imagesDeleted = false;
            if (imageUrls.Count > 0)
            {
                try
                {
                    // Use RetryHelper to wrap the entire deletion call
                    await RetryHelper.RetryOnExceptionAsync(async () =>
                    {
                        await _imageService.DeleteImageFromCloudFrontUrlAsync(imageUrls);
                    }, maxAttempts: 3, initialDelayMs: 500, cancellationToken);

                    imagesDeleted = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to delete images for post {PostId} after retries. Skipping DynamoDB delete this run.",
                        keyDict[PartitionKey].S);
                }
            }
            else
            {
                // No images to delete, so consider it "done"
                imagesDeleted = true;
            }

            // b) Only if imagesDeleted == true do we delete the DynamoDB item
            if (!imagesDeleted)
            {
                continue; // skip deletion of this record; job will try again tomorrow
            }

            // c) Now batch‐delete this single item from DynamoDB (using BatchWrite to unify logic)
            try
            {
                var writeReq = new WriteRequest
                {
                    DeleteRequest = new DeleteRequest { Key = keyDict }
                };
                var batchReq = new BatchWriteItemRequest
                {
                    RequestItems = new Dictionary<string, List<WriteRequest>>
                    {
                        { Tables.Posts, new List<WriteRequest> { writeReq } }
                    }
                };

                BatchWriteItemResponse? batchResp = await _dynamoDb.BatchWriteItemAsync(batchReq, cancellationToken);

                if (batchResp.UnprocessedItems != null
                    && batchResp.UnprocessedItems.TryGetValue(Tables.Posts, out List<WriteRequest>? unprocessed)
                    && unprocessed.Count > 0)
                {
                    // If for some reason this single‐item delete is unprocessed, retry it:
                    await RetryUnprocessedDeletesAsync(batchResp.UnprocessedItems, cancellationToken);
                }

                _logger.LogInformation("Deleted post {PostId} from DynamoDB.", keyDict[PartitionKey].S);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete post {PostId} from DynamoDB after images were removed.",
                    keyDict[PartitionKey].S);
            }
        }

        _logger.LogInformation("Finished DeleteAllInitPostsAndImagesAsync run.");
    }

    /// <summary>
    /// Exponential‐backoff retry for DynamoDB unprocessed deletes (as before).
    /// </summary>
    private async Task RetryUnprocessedDeletesAsync(
        Dictionary<string, List<WriteRequest>> unprocessed,
        CancellationToken cancellationToken)
    {
        int backoffMs = 200;
        while (unprocessed != null && unprocessed.Count > 0)
        {
            await Task.Delay(backoffMs, cancellationToken);

            var retryReq = new BatchWriteItemRequest
            {
                RequestItems = unprocessed
            };

            BatchWriteItemResponse? retryResp = await _dynamoDb.BatchWriteItemAsync(retryReq, cancellationToken);
            unprocessed = retryResp.UnprocessedItems;
            backoffMs = Math.Min(backoffMs * 2, 10_000);
        }
    }
}
