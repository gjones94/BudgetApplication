using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;

namespace BudgetApp.EmailServices
{
    public class GmailSender : IEmailSender
    {
        private readonly GmailSettings _emailSettings;
        private readonly ILogger _logger;

        public GmailSender(IOptions<GmailSettings> gmailSettings, ILogger<GmailSender> logger)
        {
            _emailSettings = gmailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromAddress));
                message.To.Add(new MailboxAddress("Recipient", email));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = htmlMessage };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                _logger.LogInformation($"Email sent to {email} successfully");
            }
            catch(Exception e)
            {
                _logger.LogError($"Email to {email} failed to send.");
                _logger.LogError($"{Environment.NewLine}Error Details:{Environment.NewLine}{e.Message}");
            }
        }

        public class GmailSettings
        {
            public string? FromAddress { get; set; }

            public string? FromName { get; set; }

            public string? SmtpServer { get; set; }

            public int SmtpPort { get; set; }

            public string? UserName { get; set;}

            public string? Password { get; set; }
        }
    }
}
