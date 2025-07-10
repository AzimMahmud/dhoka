using Application.Abstractions.Messaging;
using FluentValidation;

namespace Application.Users.Login;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<Response>;

internal sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Password).NotEmpty();
    }
}

