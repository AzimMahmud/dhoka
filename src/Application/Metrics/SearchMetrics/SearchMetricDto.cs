namespace Application.Metrics.SearchMetrics;

public class SearchMetricDto
{
    /// <summary>
    /// e.g. "1.5K", "2K", "750"
    /// </summary>
    public string TotalSearches { get; set; } = default!;

    /// <summary>
    /// e.g. "3.2K", "890"
    /// </summary>
    public string TotalPosts { get; set; } = default!;

    /// <summary>
    /// e.g. "4.1K", "120"
    /// </summary>
    public string TotalApprovedPosts { get; set; } = default!;

    /// <summary>
    /// e.g. "2M", "540"
    /// </summary>
    public string TotalSettledPosts { get; set; } = default!;
}
