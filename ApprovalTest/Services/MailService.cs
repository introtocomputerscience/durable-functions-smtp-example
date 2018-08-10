using ApprovalTest.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Security;

namespace ApprovalTest.Services
{
    public class MailService : IMailService
    {
        private SmtpClient client;
        private MailAddress sender;

        public MailService(MailAddress sender, string smtpServer, int smtpPort, SecureString password)
        {
            this.sender = sender;
            this.client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(sender.Address, password),
                EnableSsl = true
            };
        }

        public void Send(string recipient, string subject, string body, bool htmlBody = false)
        {
            MailMessage message = new MailMessage(sender, new MailAddress(recipient));
            message.Subject = subject;
            message.IsBodyHtml = htmlBody;
            message.Body = body;
            client.Send(message);
        }
    }
}
