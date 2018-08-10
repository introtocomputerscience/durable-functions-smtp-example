namespace ApprovalTest.Interfaces
{
    public interface IMailService
    {
        void Send(string recipient, string subject, string body, bool htmlBody = false);
    }
}
