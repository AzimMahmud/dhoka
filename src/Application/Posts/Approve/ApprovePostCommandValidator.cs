using FluentValidation;

namespace Application.Posts.Approve;

internal sealed class ApprovePostCommandValidator : AbstractValidator<ApprovePostCommand>
{
    public ApprovePostCommandValidator()
    {
        RuleFor(c => c.PostId).NotEmpty();
    }
}
