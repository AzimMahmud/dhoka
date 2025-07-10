using System.Text.Json.Serialization;

namespace Infrastructure.Posts;

public class TitleResponse
{
    [JsonPropertyName("title")]
    public string Title { get; set; }
}
