using FluentValidation;

namespace Application.Posts.Verify;

public class VerifyPostCommandValidator : AbstractValidator<VerifyPostCommand>
{
    public VerifyPostCommandValidator()
    {
        RuleFor(c => c.PostId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty();
        RuleFor(c => c.PaymentType).NotEmpty();
        RuleFor(c => c.Amount).NotEmpty().GreaterThan(0);
        RuleFor(c => c.Description).NotEmpty().MaximumLength(500);
    }
}
