namespace SharedKernel;

public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailModel emailModel);
}
