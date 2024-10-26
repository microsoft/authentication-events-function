using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.AuthenticationEvents;
using Microsoft.Azure.WebJobs.Extensions.AuthenticationEvents.TokenIssuanceStart;
using System.Reflection;

namespace Company.AuthEvents
{
    public static class CustomClaimsProvider
    {
        [FunctionName("CustomClaimsProvider")]
        public static WebJobsAuthenticationEventResponse Run(
             [WebJobsAuthenticationEventsTrigger] WebJobsTokenIssuanceStartRequest request, ILogger log)
        {
            try
            {
                // Checks if the request is successful and did the token validation pass
                if (request.RequestStatus == WebJobsAuthenticationEventsRequestStatusType.Successful)
                {
                    // Read the correlation ID from the Microsoft Entra request    
                    string correlationId = request.Data.AuthenticationContext.CorrelationId.ToString();

                    // Fetches information about the user from external data store
                    // Add new claims to the token's response
                    request.Response.Actions.Add(
                            new WebJobsProvideClaimsForToken(
                                new WebJobsAuthenticationEventsTokenClaim("dateOfBirth", "01/01/2000"),
                                new WebJobsAuthenticationEventsTokenClaim("customRoles", "Writer", "Editor"),
                                new WebJobsAuthenticationEventsTokenClaim("apiVersion", Assembly.GetExecutingAssembly().GetName().Version.ToString()),
                                new WebJobsAuthenticationEventsTokenClaim("correlationId", correlationId)
                                ));
                }
                else
                {
                    // If the request fails, such as in token validation, output the failed request status, 
                    // such as in token validation or response validation.
                    log.LogInformation(request.StatusMessage);
                }
                return request.Completed();
            }
            catch (Exception ex)
            {
                return request.Failed(ex);
            }
        }
    }
}
