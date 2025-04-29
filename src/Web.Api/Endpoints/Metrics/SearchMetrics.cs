using Application.Metrics.SearchMetrics;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Metrics;

internal sealed class SearchMetrics : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("metrics/search-metrics", async (ISender sender, CancellationToken cancellationToken) =>
            {
                var command = new SearchMetricsQuery();

                Result<SearchMetricDto> result = await sender.Send(command, cancellationToken);

                return result.Match(Results.Ok, CustomResults.Problem);
            })
            .WithTags(Tags.Metrics)
            // .RequireAuthorization()
            ;
    }
}
