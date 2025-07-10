using Application.Abstractions.Messaging;
using Domain.Comments;
using SharedKernel;

namespace Application.Comments.Delete;

internal sealed class DeletePostCommandHandler(ICommentRepository commentRepository)
    : ICommandHandler<DeleteCommentCommand>
{
    public async Task<Result> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
    {
        await commentRepository.DeleteAsync(command.CommentId);

        return Result.Success();
    }
}
