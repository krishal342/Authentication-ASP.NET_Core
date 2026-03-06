using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
//using System.Net.Mail;

namespace Authentication.Services
{
    public class EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        private readonly IConfiguration _config = config;
        private readonly ILogger<EmailService> _logger = logger;

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();

                // Set sender
                email.From.Add(new MailboxAddress(
                    _config["Email:FromName"],
                    _config["Email:FromAddress"]!
                ));

                // Set recipient
                email.To.Add(MailboxAddress.Parse(toEmail));

                // Set subject and body
                email.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                email.Body = bodyBuilder.ToMessageBody();

                // Send email
                using (var smtp = new SmtpClient())
                {
                    await smtp.ConnectAsync(
                        _config["Email:SmtpServer"],
                        int.Parse(_config["Email:SmtpPort"]),
                        SecureSocketOptions.StartTls
                        );

                    await smtp.AuthenticateAsync(
                        _config["Email:UserName"],
                        _config["Email:Password"]
                        );

                    await smtp.SendAsync(email);
                    await smtp.DisconnectAsync(true);

                    _logger.LogInformation($"Email sent successfully to {toEmail}");
                }

            }catch(Exception ex)
            {
                string message = $"Error sending email to {toEmail}: {ex.Message}";
                _logger.LogError(message);
                throw;
            }
        }
    }
}
