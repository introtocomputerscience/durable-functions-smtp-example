using ApprovalTest.Interfaces;
using ApprovalTest.Services;
using Autofac;
using AzureFunctions.Autofac.Configuration;
using System;
using System.Net.Mail;
using System.Security;

namespace ApprovalTest.Configs
{
    public class DIConfig
    {
        public DIConfig(string functionName)
        {
            var displayName = Environment.GetEnvironmentVariable("DisplayName");
            var smtpUsername = Environment.GetEnvironmentVariable("SMTPUsername");
            var smtpHost = Environment.GetEnvironmentVariable("SMTPHost");
            var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTPPort"));

            MailAddress sender = new MailAddress(smtpUsername, displayName);
            SecureString smtpPassword = new SecureString();
            foreach (char c in Environment.GetEnvironmentVariable("SMTPPassword"))
            {
                smtpPassword.AppendChar(c);
            }
            DependencyInjection.Initialize(builder =>
            {
                builder.Register(c => new MailService(sender, smtpHost, smtpPort, smtpPassword)).As<IMailService>();
            }, functionName);
        }
    }
}
