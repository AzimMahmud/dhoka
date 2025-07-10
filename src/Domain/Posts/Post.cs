using Amazon.DynamoDBv2.DataModel;

namespace Domain.Posts;

[DynamoDBTable("Posts")]
public class Post
{
    [DynamoDBHashKey] public Guid Id { get; init; } = Guid.NewGuid();
    
    public string? ScamType { get; set; }
    
    public string? Title { get; set; }
    
    public string? PaymentType { get; set; }

    public List<string>? MobileNumbers { get; set; } = new List<string>();
    
    public decimal? Amount { get; set; }
    
    public string? PaymentDetails { get; set; }
    

    public DateTime? ScamDateTime { get; set; }


    public string? AnonymityPreference { get; set; }

    public string? Description { get; set; }

    public string? Name { get; set; }

    public string? Status { get; set; }
    
    public string? ContactNumber { get; set; }
    
    public int Otp { get; set; }
    
    public DateTime? OtpExpirationTime { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public List<string>? ImageUrls { get; set; } = new List<string>();

    [DynamoDBIgnore]
    public bool IsApproved => string.Equals(Status, nameof(Posts.Status.Approved), StringComparison.OrdinalIgnoreCase);

    [DynamoDBIgnore]
    public bool IsSettled => string.Equals(Status, nameof(Posts.Status.Settled), StringComparison.OrdinalIgnoreCase);
    
    [DynamoDBIgnore]
    public bool IsPublic => string.Equals(AnonymityPreference, nameof(Posts.AnonymityPreference.Public), StringComparison.OrdinalIgnoreCase);
}
