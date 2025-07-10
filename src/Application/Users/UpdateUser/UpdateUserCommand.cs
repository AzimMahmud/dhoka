using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Roles;
using Domain.Users;
using FluentValidation;
using SharedKernel;

namespace Application.Users.UpdateUser;

public record UpdateUserCommand(Guid Id, string FirstName, string LastName, string Email, string Role, string Status) : ICommand<Guid>;

internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty();
        RuleFor(c => c.LastName).NotEmpty();
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Role).NotEmpty();
        RuleFor(c => c.Status).NotEmpty();
    }
}

internal class UpdateUserCommandHandler(IUserRepository userRepository, IUserContext userContext)
    : ICommandHandler<UpdateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(request.Id);

        if (user.Id == Guid.Empty)
        {
            return Result.Failure<Guid>(UserErrors.NotFound(request.Id));
        }

        if (user.Id != userContext.UserId && userContext.Role == nameof(Role.Member))
        {
            return Result.Failure<Guid>(UserErrors.Unauthorized);
        }
        
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        
        if (userContext.Role == nameof(Role.Admin))
        {
            user.Role = request.Role;
            user.Status = request.Status;
        }


        await userRepository.UpdateAsync(user);

        return Result.Success(user.Id);
    }
}



