using NpgsqlTypes;
using SharedKernel;

namespace Domain.Posts;

public class Post : Entity
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? TransactionMode { get; set; }
    public string? PaymentType { get; set; }
    public string? Description { get; set; }
    public List<string>? MobilNumbers { get; set; } = [];
    public decimal? Amount { get; set; }
    public string Status { get; set; }
    public bool IsSettled { get; set; }

    public string? ContactNumber { get; set; }

    public int Otp { get; set; }
    
    public DateTime? OtpExpirationTime { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<string>? ImageUrls { get; set; } = [];
    
    
    public NpgsqlTsVector? SearchVector { get; private set; }

    public bool IsApproved => Status == nameof(Posts.Status.Approved);
}
