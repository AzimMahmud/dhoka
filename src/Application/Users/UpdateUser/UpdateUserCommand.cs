using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using SharedKernel;

namespace Application.Users.UpdateUser;

public record UpdateUserCommand(Guid Id, string FirstName, string LastName, string Email) : ICommand<Guid>;

internal class UpdateUserCommandHandler(IApplicationDbContext context, IUserContext userContext) : ICommandHandler<UpdateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        
        
        User? user = await context.Users.FindAsync(request.Id, cancellationToken);

        if (user is null)
        {
            return Result.Failure<Guid>(UserErrors.NotFound(request.Id));
        }

        if (user.Id != userContext.UserId)
        {
            return Result.Failure<Guid>(UserErrors.Unauthorized());
        }
        

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
