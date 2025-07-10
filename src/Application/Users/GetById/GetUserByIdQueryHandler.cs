using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Users;
using SharedKernel;

namespace Application.Users.GetById;

internal sealed class GetUserByIdQueryHandler(IUserRepository userRepository, IUserContext userContext)
    : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {

        User? user = await userRepository.GetByIdAsync(query.UserId);

        if (user.Id == Guid.Empty)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound(query.UserId));
        }

        var userResponse = new UserResponse()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Status = user.Status,
        };
        return userResponse;
    }
}
