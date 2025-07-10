using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Tokens;
using Domain.Users;
using SharedKernel;

namespace Application.Users.RevokeRefreshTokens;

internal sealed class RevokeRefreshTokenCommandHandler(IRefreshTokenRepository refreshTokenRepository, IUserContext userContext)
    : ICommandHandler<RevokeRefreshTokenCommand, bool>
{
    public async Task<Result<bool>> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure<bool>(UserErrors.Unauthorized);
        }
        
        try
        {
            await refreshTokenRepository.DeleteByUserIdAsync(request.UserId);
        }
        catch (Exception e)
        {
            return false;
        }

        return true;
    }
}
