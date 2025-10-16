using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HR_Arvius.Services
{
    public class EmailService
    {
        private readonly HR_Arvius.Configuration.EmailSettings _settings;

        public EmailService(IOptions<HR_Arvius.Configuration.EmailSettings> settings)
        {
            _settings = settings.Value;
        }
        public void SendEmailFireAndForget(string toEmail, string subject, string htmlBody, string[]? cc = null)
        {
            // Run email sending in a background thread
            Task.Run(async () =>
            {
                try
                {
                    await SendEmailAsync(toEmail, subject, htmlBody, cc);
                }
                catch (Exception ex)
                {
                    // Log failure, but do not crash the app
                    Console.WriteLine($"Email sending failed: {ex.Message}");
                }
            });
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string[]? cc)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));

            // Add CC recipients if provided
            if (cc != null && cc.Length > 0)
            {
                foreach (var ccAddress in cc)
                {
                    if (!string.IsNullOrWhiteSpace(ccAddress))
                        message.Cc.Add(MailboxAddress.Parse(ccAddress));
                }
            }

            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}
