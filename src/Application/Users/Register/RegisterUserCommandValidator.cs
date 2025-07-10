using FluentValidation;

namespace Application.Users.Register;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.FirstName)
            .NotEmpty().WithMessage("First name is required.");

        RuleFor(c => c.LastName)
            .NotEmpty().WithMessage("Last name is required.");

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(c => c.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(c => c.Password).WithMessage("Password and confirm password don't match.");
    }
}
