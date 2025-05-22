using Amazon.DynamoDBv2.DataModel;

namespace Domain.Posts;

[DynamoDBTable("Posts")]
public class Post
{
    [DynamoDBHashKey]
    public Guid Id { get; set; }

    public string? Title { get; set; }
    public string? TransactionMode { get; set; }
    public string? PaymentType { get; set; }
    public string? Description { get; set; }
    public List<string>? MobilNumbers { get; set; } = [];
    public decimal? Amount { get; set; }
    public string Status { get; set; }
    public bool? IsSettled { get; set; }
    public string? ContactNumber { get; set; }
    public int Otp { get; set; }
    public DateTime? OtpExpirationTime { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<string>? ImageUrls { get; set; } = [];

    // Computed property, not stored in DynamoDB
    [DynamoDBIgnore]
    public bool IsApproved => Status == "Approved";
}
