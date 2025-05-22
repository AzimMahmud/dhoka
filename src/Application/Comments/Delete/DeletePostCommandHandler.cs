using Application.Abstractions.Messaging;
using Domain.Comments;
using SharedKernel;

namespace Application.Comments.Delete;

internal sealed class DeletePostCommandHandler(ICommentRepository commentRepository)
    : ICommandHandler<DeleteCommentCommand>
{
    public async Task<Result> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
    {
        // Comment? comment = await context.Comments
        //     .SingleOrDefaultAsync(t =>  t.Id == command.CommentId, cancellationToken);
        //
        // if (comment is null)
        // {
        //     return Result.Failure(CommentErrors.NotFound(command.CommentId));
        // }
        //
        // context.Comments.Remove(comment);
        //
        // await context.SaveChangesAsync(cancellationToken);
        
        await commentRepository.DeleteAsync(command.CommentId);

        return Result.Success();
    }
}
