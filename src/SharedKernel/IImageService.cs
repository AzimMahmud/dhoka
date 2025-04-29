using Microsoft.AspNetCore.Http;

namespace SharedKernel;

public interface IImageService
{
    Task<List<string>> UploadImagesAsync(IFormFileCollection file, Guid postId);
    Task DeleteImageFromCloudFrontUrlAsync(List<string> cloudFrontUrls);
}
