using Application.Abstractions.Messaging;

namespace Application.Posts.Verify;

public sealed class VerifyPostCommand : ICommand<Guid>
{
    public Guid PostId { get; set;}
    public int Otp { get; set;}
    public string Title { get; set;}
    public string TransactionMode { get; set;}
    public string PaymentType { get; set;}
    public string Description { get; set;}
    public List<string> MobileNumbers { get; set; }
    public decimal Amount { get; set; }
}
