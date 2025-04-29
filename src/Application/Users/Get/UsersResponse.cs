namespace Application.Users.Get;

public sealed record UsersResponse(Guid Id, string Email, string FirstName, string LastName);
