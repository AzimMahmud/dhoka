namespace Domain;

public class PagedSearchResult<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = new List<T>();
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public long TotalCount { get; set; }
}

public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = new List<T>();
    public string? NextPageToken { get; set; }
    
    public bool HasMore { get; set; } 
}
