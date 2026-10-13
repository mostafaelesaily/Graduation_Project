using MimeKit;
using MailKit.Net.Smtp;
using MailKit;
using MedLink.Application.Interfaces.Common;
namespace MedLink.infrastructure.Services
{
    public class MailService : Application.Interfaces.Common.IMailService
    {
        public async Task sendMail(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("MedLink", "sinacola58@gmail.com"));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = body };

            using (var smtp = new SmtpClient())
            {
                await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync("sinacola58@gmail.com", "magu keqn xjqe widj");
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
        }

    }
}
