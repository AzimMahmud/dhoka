using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Roles;
using Domain.Users;
using SharedKernel;

namespace Application.Users.DeleteUser;

public record DeleteUserCommand(Guid Id) : ICommand<Guid>;

internal class DeleteUserCommandHandler(IUserRepository userRepository, IUserContext userContext)
    : ICommandHandler<DeleteUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.GetByIdAsync(request.Id);

        if (user.Id == Guid.Empty)
        {
            return Result.Failure<Guid>(UserErrors.NotFound(request.Id));
        }

        if (userContext.Role == nameof(Role.Member))
        {
            return Result.Failure<Guid>(UserErrors.Unauthorized);
        }
        
        user.Status = nameof(Status.Inactive);
        
        await userRepository.UpdateAsync(user);

        return Result.Success(user.Id);
    }
}
