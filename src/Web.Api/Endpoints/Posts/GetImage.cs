using Application.Posts.GetImage;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Posts;

public sealed class GetImage : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("posts/{id:guid}/images", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new GetImageQuery(id);
                
                Result<List<string>> result = await sender.Send(command, cancellationToken);
                
                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithTags(Tags.Posts);
    }
}
