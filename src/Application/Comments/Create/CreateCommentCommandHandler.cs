using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Comments;
using SharedKernel;

namespace Application.Comments.Create;

internal sealed class CreateCommentCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateCommentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCommentCommand command, CancellationToken cancellationToken)
    {
        var comment = new Comment
        {
            PostId = command.PostId,
            ContactInfo = command.ContactInfo,
            Description = command.Description,
            CreatedAt = dateTimeProvider.UtcNow
        };
        
        context.Comments.Add(comment);

        await context.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }
}
