using SharedKernel;

namespace Domain.SearchEvents;

public class SearchEvent : Entity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Query { get; set; }
}
