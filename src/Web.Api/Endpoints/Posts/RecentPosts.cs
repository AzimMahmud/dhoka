using Application.Posts.RecentPosts;
using Domain;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Posts;

internal sealed class RecentPosts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("posts/recent-posts", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var query = new RecentPostsQuery();

                Result<List<PostsResponse>> result = await sender.Send(query, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithTags(Tags.Posts);
    }
}
