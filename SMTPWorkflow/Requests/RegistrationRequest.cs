using System;
using System.Collections.Generic;
using System.Text;

namespace ApprovalTest.Requests
{
    public class RegistrationRequest : ApprovalRequest
    {
        public bool Approved { get; set; }
        public RegistrationRequest() { }
        public RegistrationRequest(ApprovalRequest request)
        {
            this.Requestor = request.Requestor;
            this.Email = request.Email;
        }
    }
}
