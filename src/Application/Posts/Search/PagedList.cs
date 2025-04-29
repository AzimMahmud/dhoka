namespace Application.Posts.Search;

public class SearchPagedList
{
    public SearchPagedList()
    {
        
    }
    private SearchPagedList(List<SearchPostsResponse> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public List<SearchPostsResponse> Items { get; }

    public int Page { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public bool HasNextPage => Page * PageSize < TotalCount;

    public bool HasPreviousPage => Page > 1;

    public static SearchPagedList CreateAsync(List<SearchPostsResponse> items, int page, int pageSize, int totalCount)
    {
        return new(items, page, pageSize, totalCount);
    }
}
