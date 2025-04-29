namespace Application.Metrics.SearchMetrics;

public record SearchMetricDto
{
    public int TotalSearches {get; set;}
    public int TotalPosts {get; set;}
    public int TotalApprovedPosts {get; set;}
    public int TotalSettledPosts {get; set;}
}
