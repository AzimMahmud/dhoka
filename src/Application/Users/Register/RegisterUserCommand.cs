﻿using Application.Abstractions.Messaging;

namespace Application.Users.Register;

public sealed record RegisterUserCommand(string Email, string FirstName, string LastName, string Password, string ConfirmPassword, string Role )
    : ICommand<Guid>;
