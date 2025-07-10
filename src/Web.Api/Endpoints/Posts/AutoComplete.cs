using Application.Posts.AutoComplete;
using Domain;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Posts;

internal sealed class AutoComplete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("posts/autocomplete", async ([AsParameters]AutocompleteRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new AutoCompleteQuery(request);
 
                Result<List<string>> result = await sender.Send(command, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithTags(Tags.Posts);
    }
}
