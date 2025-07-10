using FluentValidation;

namespace Application.Posts.Admin.ReIndex;

internal sealed class ReIndexPostCommandValidator : AbstractValidator<ReIndexPostCommand>
{
    public ReIndexPostCommandValidator()
    {
        RuleFor(c => c.PostId).NotEmpty();
    }
}
