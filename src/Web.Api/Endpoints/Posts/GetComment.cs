using Application.Posts.Comment;
using Domain.Comments;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Posts;

internal sealed class GetComments : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("posts/{id:guid}/comments", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new GetCommentQuery(id);

            Result<List<CommentsResponse>> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Posts);
    }
}
