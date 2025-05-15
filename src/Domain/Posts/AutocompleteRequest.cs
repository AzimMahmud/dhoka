namespace Domain.Posts;

public class AutocompleteRequest
{
    public string Prefix { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 5;
}
