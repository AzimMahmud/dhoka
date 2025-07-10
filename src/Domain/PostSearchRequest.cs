namespace Domain;

public class PostSearchRequest
{
    public string? SearchTerm { get; set; }
    public int PageSize { get; set; } = 10;

    public int CurrentPage { get; set; } = 1;
}
