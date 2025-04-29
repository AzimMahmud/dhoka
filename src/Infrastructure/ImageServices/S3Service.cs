using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SharedKernel;

namespace Infrastructure.ImageServices;

public class S3Service(IAmazonS3 s3Client, IAmazonCloudFront cloudFrontClient, IConfiguration configuration)
    : IImageService
{
    private readonly string? _bucketName = configuration["AWS:BucketName"];
    private readonly string? _distributionId = configuration["AWS:DistributionId"];
    private readonly string? _cloudFrontDomain = configuration["AWS:CloudFrontDomain"];

    public async Task<List<string>> UploadImagesAsync(IFormFileCollection files, Guid postId)
    {
        IEnumerable<Task<string>> uploadTasks = files.Select(async file =>
        {
            string objectKey = $"{postId}/{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                InputStream = file.OpenReadStream(),
                ContentType = file.ContentType,
                CannedACL = S3CannedACL.Private
            };

           PutObjectResponse? res = await s3Client.PutObjectAsync(request);
            return $"https://{_cloudFrontDomain}/{objectKey}";
        });

        return (await Task.WhenAll(uploadTasks)).ToList();
    }

    public async Task DeleteImageFromCloudFrontUrlAsync(List<string> cloudFrontUrls)
    {
        if (cloudFrontUrls == null || !cloudFrontUrls.Any())
        {
            throw new ArgumentException("No URLs provided for deletion.");
        }

        var s3Keys = new List<string>();

        // Step 1: Extract S3 keys from CloudFront URLs
        foreach (string cloudFrontUrl in cloudFrontUrls)
        {
            if (string.IsNullOrEmpty(cloudFrontUrl) || !cloudFrontUrl.StartsWith($"https://{_cloudFrontDomain}/"))
            {
                throw new ArgumentException($"Invalid CloudFront URL: {cloudFrontUrl}");
            }

            string s3Key = GetS3KeyFromCloudFrontUrl(cloudFrontUrl);
            s3Keys.Add(s3Key);
        }

        // Step 2: Delete objects from S3 in bulk
        if (s3Keys.Any())
        {
            var deleteObjectsRequest = new DeleteObjectsRequest
            {
                BucketName = _bucketName,
                Objects = s3Keys.Select(key => new KeyVersion { Key = key }).ToList()
            };
            await s3Client.DeleteObjectsAsync(deleteObjectsRequest);
        }

        // Step 3: Invalidate CloudFront cache in batch
        await InvalidateCloudFrontCacheAsync(s3Keys);
    }

    private string GetS3KeyFromCloudFrontUrl(string cloudFrontUrl)
    {
        return cloudFrontUrl.Replace($"https://{_cloudFrontDomain}/", "");
    }

    private async Task InvalidateCloudFrontCacheAsync(List<string> keys)
    {
        if (!keys.Any())
        {
            return;
        }

        var invalidationRequest = new CreateInvalidationRequest
        {
            DistributionId = _distributionId,
            InvalidationBatch = new InvalidationBatch
            {
                Paths = new Paths
                {
                    Quantity = keys.Count,
                    Items = keys.Select(key => $"/{key}").ToList()
                },
                CallerReference = Guid.NewGuid().ToString()
            }
        };
        await cloudFrontClient.CreateInvalidationAsync(invalidationRequest);
    }
}
