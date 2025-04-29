using Application.Abstractions;

namespace Application.Posts.AutoComplete;

public class AutoCompleteResponse
{
    public string? Title { get; set; }
    public string? TransactionMode { get; set; }
    public string? PaymentType { get; set; }
    public string? Description { get; set; }
}
