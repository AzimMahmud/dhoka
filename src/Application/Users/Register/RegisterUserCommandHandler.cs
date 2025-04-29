using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.VerifyEmail;
using Domain.Roles;
using Domain.Tokens;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Quartz;
using SharedKernel;


namespace Application.Users.Register;

internal sealed class RegisterUserCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IEmailService emailService,
    IConfiguration configuration)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken: cancellationToken))
        {
            throw new ApplicationException("The email is already in use");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = passwordHasher.Hash("Demo@1234")
        };
        context.Users.Add(user);

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = Role.MemberId
        };
        context.UserRoles.Add(userRole);

        DateTime utcNow = DateTime.UtcNow;
        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CreatedOnUtc = utcNow,
            ExpiresOnUtc = utcNow.AddDays(1)
        };

        context.EmailVerificationTokens.Add(verificationToken);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException e)
            when (e.InnerException is NpgsqlException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Result<Guid>.ValidationFailure(UserErrors.EmailNotUnique);
        }

        string verificationLink =  $"{configuration["ClientAppUrl"]}/verify-email?token={verificationToken.Id}";


        string emailBody = $@"
            <p>Hello,</p>

            <p>Thank you for registering! Please confirm your email by clicking the link below:</p>

            <p><a href='{verificationLink}'>Confirm your email address</a></p>

            <p>If you did not create an account, please ignore this email.</p>

            <p>Thanks,<br/>The Dhoka Team</p>
        ";

       bool res =  await emailService.SendEmailAsync(new EmailModel
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
