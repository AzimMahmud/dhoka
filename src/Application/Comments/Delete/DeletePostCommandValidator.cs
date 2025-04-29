using FluentValidation;

namespace Application.Comments.Delete;

internal sealed class DeletePostCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeletePostCommandValidator()
    {
        RuleFor(c => c.CommentId).NotEmpty();
    }
}
