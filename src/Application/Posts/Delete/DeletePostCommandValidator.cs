using FluentValidation;

namespace Application.Posts.Delete;

internal sealed class DeletePostCommandValidator : AbstractValidator<DeletePostCommand>
{
    public DeletePostCommandValidator()
    {
        RuleFor(c => c.PostId).NotEmpty();
    }
}
