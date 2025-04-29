using Application.Posts.Reject;
using Application.Posts.UploadImages;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Posts;

internal sealed class UploadImages : IEndpoint
{
    public sealed class Request
    {
        [FromForm(Name = "images")]
        public IFormFileCollection Images { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("posts/{id:guid}/upload-images",
                async (Guid id,  IFormFileCollection images, ISender sender, CancellationToken cancellationToken) =>
                {
                    var command = new UploadImagesCommand { PostId = id, Images = images };
        
                    Result result = await sender.Send(command, cancellationToken);
        
                    return result.Match(Results.NoContent, CustomResults.Problem);
                })
            .DisableAntiforgery()
            .WithTags(Tags.Posts);
    }
}
