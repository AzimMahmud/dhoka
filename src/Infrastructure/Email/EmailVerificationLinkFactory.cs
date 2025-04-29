using Domain.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Infrastructure.Email;

internal sealed class EmailVerificationLinkFactory(
    IHttpContextAccessor httpContextAccessor,
    LinkGenerator linkGenerator)
{
    public const string VerifyEmailName = "VerifyEmail";
    
    public string Create(EmailVerificationToken emailVerificationToken)
    {
        string? verificationLink = linkGenerator.GetUriByName(
            httpContextAccessor.HttpContext!,
            VerifyEmailName,
            new { token = emailVerificationToken.Id });

        return verificationLink ?? throw new Exception("Could not create email verification link");
    }
}
