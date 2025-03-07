using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public class CustomClaimsProvider
    {
        private readonly ILogger<CustomClaimsProvider> _logger;

        public CustomClaimsProvider(ILogger<CustomClaimsProvider> logger)
        {
            _logger = logger;
        }

        [Function("OnTokenIssuanceStart_CustomClaimsProvider")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // Get the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            JsonNode jsonPayload = JsonNode.Parse(requestBody)!;

            // Read the user principal name (UPN) and the correlation ID from the Microsoft Entra request
            string upn = jsonPayload["data"]!["authenticationContext"]!["user"]!["userPrincipalName"]!.ToString();
            string correlationId = jsonPayload["data"]!["authenticationContext"]!["correlationId"]!.ToString();

            // Placeholder to retrive information from interanl systems
            // For example, using the user's UPN, you can call a database or an API to get the user's roles
            string dateOfBirth = "01/01/2000";
            List<string> customRoles = new List<string>() { "Writer", "Editor" };

            // Prepare a response object
            ResponseObject responseData = new ResponseObject("microsoft.graph.onTokenIssuanceStartResponseData");
            Claims claims = new Claims();

            // Return the user attributes from the internal system
            claims.DateOfBirth = dateOfBirth;
            claims.CustomRoles = customRoles;

            // Return the correlation ID and the API version for debugging purposes
            claims.CorrelationId = correlationId;
            claims.ApiVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();

            // Define the "action type" and the "claims" to be returned to Microsoft Entra
            responseData.Data.Actions = new List<ResponseAction>() { new ResponseAction(
                "microsoft.graph.tokenIssuanceStart.provideClaimsForToken",
                claims) };

            return new OkObjectResult(responseData);
        }
    }

    public class ResponseObject
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }

        public ResponseObject(string dataType)
        {
            Data = new Data(dataType);
        }
    }

    public class Data
    {
        [JsonPropertyName("@odata.type")]
        public string DataType { get; set; }
        [JsonPropertyName("actions")]
        public List<ResponseAction> Actions { get; set; }

        public Data(string dataType)
        {
            DataType = dataType;
        }
    }

    public class ResponseAction
    {
        [JsonPropertyName("@odata.type")]
        public string DataType { get; set; }

        [JsonPropertyName("claims")]
        public Claims Claims { get; set; }

        public ResponseAction(string dataType, Claims claims)
        {
            DataType = dataType;
            Claims = claims;
        }
    }

    public class Claims
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CorrelationId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DateOfBirth { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ApiVersion { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? CustomRoles { get; set; }
    }
}
