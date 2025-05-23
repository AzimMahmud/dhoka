﻿using FluentValidation;

namespace Application.Posts.Reject;

internal sealed class RejectPostCommandValidator : AbstractValidator<RejectPostCommand>
{
    public RejectPostCommandValidator()
    {
        RuleFor(c => c.PostId).NotEmpty();
    }
}
