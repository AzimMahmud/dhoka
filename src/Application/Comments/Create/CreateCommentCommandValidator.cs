using FluentValidation;

namespace Application.Comments.Create;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(c => c.PostId).NotEmpty();
        RuleFor(c => c.ContactInfo).NotEmpty();
        RuleFor(c => c.Description).NotEmpty().MaximumLength(500);
    }
}
