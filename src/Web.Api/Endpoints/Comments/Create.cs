using Application.Comments.Create;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Comments;

internal sealed class Create : IEndpoint
{
    public sealed class Request
    {
        public Guid PostId { get; set; }
    
        public string ContactInfo { get; set; }
        public string Description { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("comments", async (Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new CreateCommentCommand
            {
                PostId = request.PostId,
                Description = request.Description,
                ContactInfo = request.ContactInfo
            };

            Result<Guid> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Comments);
    }
}
