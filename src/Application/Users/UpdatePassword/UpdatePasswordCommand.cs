using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using SharedKernel;

namespace Application.Users.UpdatePassword;

public record UpdatePasswordCommand(Guid Id, string Password, string ConfirmPassword) : ICommand<Guid>;

internal class UpdatePasswordCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IUserContext userContext) : ICommandHandler<UpdatePasswordCommand, Guid>
{
    public async Task<Result<Guid>> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
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
        
        
        if (request.Password != request.ConfirmPassword)
        {
            return Result.Failure<Guid>(UserErrors.PasswordNotMatch);
        }

        user.PasswordHash = passwordHasher.Hash(request.Password);
        
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}
