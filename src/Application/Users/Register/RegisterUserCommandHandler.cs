using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Roles;
using Domain.Tokens;
using Domain.Users;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SharedKernel;


namespace Application.Users.Register;

internal sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IEmailVerificationTokenRepository emailVerificationTokenRepository,
    IPasswordHasher passwordHasher,
    IEmailService emailService,
    IConfiguration configuration)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        User existingUser = await userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new ApplicationException("The email is already in use");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = passwordHasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow,
            Role = !string.IsNullOrEmpty(request.Role) ? request.Role :nameof(Role.Member),
            Status = nameof(Status.Active)
        };

        await userRepository.CreateAsync(user);

        DateTime utcNow = DateTime.UtcNow;
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CreatedOnUtc = utcNow,
            ExpiresOnUtc = utcNow.AddDays(1)
        };

        await emailVerificationTokenRepository.CreateAsync(verificationToken);

        string verificationLink = $"{configuration["ClientAppUrl"]}/verify-email?token={verificationToken.Id}";
        
        string emailBody = $@"
            <p>Hello,</p>

            <p>Thank you for registering! Please confirm your email by clicking the link below:</p>

            <p><a href='{verificationLink}'>Confirm your email address</a></p>

            <p>If you did not create an account, please ignore this email.</p>

            <p>Thanks,<br/>The Dhoka Team</p>
        ";

        await emailService.SendEmailAsync(new EmailModel
        {
            From = "azimmahamud@gmail.com",
            ToEmail = user.Email,
            Subject = "User account verification",
            Body = emailBody,
            IsHtml = false
        });

        return user.Id;
    }
}
