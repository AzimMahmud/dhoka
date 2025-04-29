using System.Net;
using System.Net.Mail;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Mailjet.Client.TransactionalEmails;
using Mailjet.Client.TransactionalEmails.Response;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SharedKernel;

namespace Infrastructure.MessageServices;

public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly string _apiKey;
        private readonly string _secretKey;
        public EmailService(ILogger<EmailService> logger, IConfiguration configuration) 
        {
            _logger = logger;
            _apiKey = configuration["EmailApi:API_Key"];
            _secretKey = configuration["EmailApi:Secret_Key"];
        }

        public async Task<bool> SendEmailAsync(EmailModel emailModel)
        {
            try
            {
                var client = new MailjetClient(_apiKey, _secretKey);

                var request = new MailjetRequest
                {
                    Resource = Send.Resource
                };

                //format email
                emailModel.Body = await FormatEmailTemplate(emailModel.From, emailModel.Subject, emailModel.Body);

                TransactionalEmail? email = new TransactionalEmailBuilder()
                       .WithFrom(new SendContact(emailModel.From))
                       .WithSubject(emailModel.Subject)
                       .WithHtmlPart(emailModel.Body)
                       .WithTo(new SendContact(emailModel.ToEmail))
                       .Build();

                TransactionalEmailResponse? response = await client.SendTransactionalEmailAsync(email);
                MessageResult? message = response.Messages[0];

                bool result = message.Status.ToLower() == "success";

                _logger.LogInformation("Email sent successfully.");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email.");

                return false;
            }
        }

        private async Task<string> FormatEmailTemplate(string fromEmail, string subject, string message)
        {
            string filePath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $"Files/email.html"));
            string template = await File.ReadAllTextAsync($"{filePath}");

            template = template.Replace("{fromEmail}", fromEmail);
            template = template.Replace("{subject}", subject);
            template = template.Replace("{message}", message);

            return template;
        }
    }
