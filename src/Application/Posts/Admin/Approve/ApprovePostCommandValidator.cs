using FluentValidation;

namespace Application.Posts.Admin.Approve;

internal sealed class ApprovePostCommandValidator : AbstractValidator<ApprovePostCommand>
{
    public ApprovePostCommandValidator()
    {
        RuleFor(c => c.PostId).NotEmpty();
    }
}
