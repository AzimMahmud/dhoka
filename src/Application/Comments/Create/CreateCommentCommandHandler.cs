using Application.Abstractions.Messaging;
using Domain.Comments;
using SharedKernel;

namespace Application.Comments.Create;

internal sealed class CreateCommentCommandHandler(
    ICommentRepository commentRepository)
    : ICommandHandler<CreateCommentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCommentCommand command, CancellationToken cancellationToken)
    {
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PostId = command.PostId,
            ContactInfo = command.ContactInfo,
            Description = command.Description,
            CreatedAt = DateTime.UtcNow
        };
        await commentRepository.CreateAsync(comment);
        return comment.Id;
    }
}
