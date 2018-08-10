using ApprovalTest.Models;

namespace ApprovalTest.Requests
{
    public class ApprovalRequest
    {
        public Person Requestor { get; set; }
        public string Email { get; set; }
        public string CorrelationId { get; set; }
    }
}
