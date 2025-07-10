using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Users;
using FluentValidation;
using SharedKernel;

namespace Application.Users.UpdatePassword;

public record UpdatePasswordCommand(Guid Id, string Password, string ConfirmPassword) : ICommand<Guid>;

internal sealed class UpdatePasswordCommandValidator : AbstractValidator<UpdatePasswordCommand>
{
    public UpdatePasswordCommandValidator()
    {
        RuleFor(c => c.Password).NotEmpty();
        RuleFor(c => c.ConfirmPassword).NotEmpty();
    }
}


internal class UpdatePasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUserContext userContext) : ICommandHandler<UpdatePasswordCommand, Guid>
{
    public async Task<Result<Guid>> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(request.Id);

        if (user.Id == Guid.Empty)
        {
            return Result.Failure<Guid>(UserErrors.NotFound(request.Id));
        }

        if (user.Id != userContext.UserId)
        {
            return Result.Failure<Guid>(UserErrors.Unauthorized);
        }
        
        if (request.Password != request.ConfirmPassword)
        {
            return Result.Failure<Guid>(UserErrors.PasswordNotMatch);
        }

        user.PasswordHash = passwordHasher.Hash(request.Password);

        await userRepository.UpdateAsync(user);

        return Result.Success(user.Id);
    }
}
