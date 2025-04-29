using Application.Abstractions.Messaging;

namespace Application.Comments.Create;

public sealed class CreateCommentCommand : ICommand<Guid>
{
    public Guid PostId { get; set; }
    
    public string ContactInfo { get; set; }
    public string Description { get; set; }
}
