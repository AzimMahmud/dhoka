using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.RevokeRefreshTokens;

internal sealed class RevokeRefreshTokenCommandHandler(IApplicationDbContext context, IUserContext userContext)
    : ICommandHandler<RevokeRefreshTokenCommand, bool>
{
    public async Task<Result<bool>> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure<bool>(UserErrors.Unauthorized());
        }

        try
        {
            await context.RefreshTokens
                .Where(r => r.UserId == request.UserId).ExecuteDeleteAsync(cancellationToken);
        }
        catch (Exception e)
        {
            return false;
        }

        return true;
    }
}
