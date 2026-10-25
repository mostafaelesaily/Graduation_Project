using MimeKit;
using MailKit.Net.Smtp;
using MailKit;
using MedLink.Application.Interfaces.Common;
using Microsoft.Extensions.Configuration;
namespace MedLink.infrastructure.Services
{
    public class MailService : Application.Interfaces.Common.IMailService
    {
        public MailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        private readonly IConfiguration configuration;

        public async Task sendMail(string to, string subject, string body)
        {
            var senderName = configuration["Mail:SenderName"];
            var username = configuration["Mail:Username"];
            var password = configuration["Mail:Password"];
            var host = configuration["Mail:Host"];
            var port = int.Parse(configuration["Mail:Port"]!);

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(senderName, username));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using (var smtp = new SmtpClient())
            {
                await smtp.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(username, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
        }

    }
}
