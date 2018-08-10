using ApprovalTest.Configs;
using ApprovalTest.Interfaces;
using ApprovalTest.Requests;
using ApprovalTest.Responses;
using AzureFunctions.Autofac;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace ApprovalTest
{
    [DependencyInjectionConfig(typeof(DIConfig))]
    public static class ApprovalWorkflow
    {
        [FunctionName("RequestApproval")]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var request = context.GetInput<ApprovalRequest>();
            request.CorrelationId = context.InstanceId;
            //Start Approval
            var confirmationCode = await context.CallActivityAsync<string>("SendApprovalRequest", request);

            //Wait 15 minutes for Approval otherwise Reject
            using(var cancellationTokenSource = new CancellationTokenSource())
            {
                DateTime expireTime = context.CurrentUtcDateTime.AddMinutes(15);
                Task timeoutTask = context.CreateTimer(expireTime, cancellationTokenSource.Token);

                Task<ConfirmationResponse> approvalResponseTask = context.WaitForExternalEvent<ConfirmationResponse>("ReceiveApprovalResponse");

                Task collector = await Task.WhenAny(approvalResponseTask, timeoutTask);

                var registrationRequest = new RegistrationRequest(request);
                if (collector == approvalResponseTask)
                {
                    if (approvalResponseTask.Result.Approved && approvalResponseTask.Result.ConfirmationCode == confirmationCode)
                    {
                        registrationRequest.Approved = true;
                    }
                    else
                    {
                        registrationRequest.Approved = false;
                    }
                }
                else
                {
                    //Event timed out
                    registrationRequest.Approved = false;
                }

                //Cancel the timer just in case
                if (!timeoutTask.IsCompleted) { cancellationTokenSource.Cancel(); }

                var registrationStatus = await context.CallActivityAsync<string>("CompleteRegistration", registrationRequest);
                return registrationStatus;
            }
        }

        [FunctionName("SendApprovalRequest")]
        public static string SendApprovalRequest([ActivityTrigger] ApprovalRequest request, ExecutionContext context, ILogger log, [Inject]IMailService mailService)
        {
            var confirmationCode = RandomString(6);
            string html = File.ReadAllText($"{context.FunctionAppDirectory}\\Templates\\template.html");
            var baseAddress = $"http://localhost:7071/api/verify?corelationId={request.CorrelationId}&confirmationCode={confirmationCode}";
            string body = html.Replace("@confirmLink", $"{baseAddress}&action=confirm")
                              .Replace("@rejectLink", $"{baseAddress}&action=deny")
                              .Replace("@confirmationCode", confirmationCode);
            mailService.Send(request.Email, "Email Verification from Durable Functions", body, true);
            log.LogInformation("Message Sent");
            return confirmationCode;
        }

        [FunctionName("ReceiveApprovalResponse")]
        public static async Task<HttpResponseMessage> ReceiveApprovalResponse([HttpTrigger(AuthorizationLevel.Function, methods: "get", Route = "verify")] HttpRequestMessage req, 
                                                                              [OrchestrationClient] DurableOrchestrationClient orchestrationClient, 
                                                                              ILogger log)
        {
            log.LogInformation($"Received an E-Mail Confirmation");
            string instanceId = req.RequestUri.ParseQueryString().GetValues("corelationId")[0];
            string confirmationCode = req.RequestUri.ParseQueryString().GetValues("confirmationCode")[0];
            string action = req.RequestUri.ParseQueryString().GetValues("action")[0];
            log.LogInformation($"InstanceId: {instanceId}, Confirmation Code: {confirmationCode}, Action: {action}");
            var status = await orchestrationClient.GetStatusAsync(instanceId);
            log.LogInformation($"Orchestration status: {status}");
            if (status != null && (status.RuntimeStatus == OrchestrationRuntimeStatus.Running || status.RuntimeStatus == OrchestrationRuntimeStatus.Pending))
            {
                bool isApproved = false;
                string message = "The request has been cancelled.";
                if (action.ToLower() == "confirm")
                {
                    isApproved = true;
                    message = "Thanks for confirming your email!";
                }
                var request = new ConfirmationResponse()
                {
                    Approved = isApproved,
                    ConfirmationCode = confirmationCode
                };
                await orchestrationClient.RaiseEventAsync(instanceId, "ReceiveApprovalResponse", request);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(message) };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Whoops! Something went wrong!") };
            }
        }

        [FunctionName("CompleteRegistration")]
        public static string CompleteRegistration([ActivityTrigger] RegistrationRequest request, ILogger log)
        {
            string response;
            if (request.Approved)
            {
                //Approved - Mark user as verified in Database
                response = "Registration successful";
            }
            else
            {
                //Denied - Mark user as unverified in Database
                response = "Registration failed";
            }
            return response;
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}