using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Http;

namespace Application.Posts.UploadImages;

public sealed class UploadImagesCommand : ICommand
{
    public Guid PostId { get; set; }
    public IFormFileCollection Images { get; set; }
}
