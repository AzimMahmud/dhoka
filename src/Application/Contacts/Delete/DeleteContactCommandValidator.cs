using FluentValidation;

namespace Application.Contacts.Delete;

internal sealed class DeleteContactCommandValidator : AbstractValidator<DeleteContactCommand>
{
    public DeleteContactCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
