using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.AuthEvents.OnAttributeCollectionSubmit.ShowValidationError
{
    public class ShowValidationError
    {
        private readonly ILogger<ShowValidationError> _logger;

        public ShowValidationError(ILogger<ShowValidationError> logger)
        {
            _logger = logger;
        }

        [Function("OnAttributeCollectionSubmit_ShowValidationError")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // Get the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            JsonNode jsonPayload = JsonNode.Parse(requestBody)!;

            // Get sign-up values
            JsonNode attributes = jsonPayload["data"]!["userSignUpInfo"]!["attributes"]!;

            // Init response data
            ResponseObject responseData = new ResponseObject("microsoft.graph.onAttributeCollectionSubmitResponseData");
            var attributeErrors = new Dictionary<string, string>();

            // Check the city attribute
            if (attributes["city"] == null || attributes["city"]!["value"] == null || attributes["city"]!["value"]!.ToString().Length < 7)
            {
                attributeErrors.Add("city", "Length of city string should be of at 5 characters at least");
            }

            // Check the postal code attribute
            if (attributes["postalCode"] == null || attributes["postalCode"]!["value"] == null || attributes["postalCode"]!["value"]!.ToString().Length < 7)
            {
                attributeErrors.Add("postalCode", "Length of postalCodeValue string should be of at 5 characters at least");
            }

            // Check the street address attribute
            if (attributes["streetAddress"] == null || attributes["streetAddress"]!["value"] == null || attributes["streetAddress"]!["value"]!.ToString().Length < 7)
            {
                attributeErrors.Add("streetAddress", "Length of streetAddress string should be of at 5 characters at least");
            }

            // Check if there is any error to return
            if (attributeErrors.Count > 0)
            {
                responseData.Data.Actions = new List<ResponseAction>() { new ResponseAction(
                    "microsoft.graph.attributeCollectionSubmit.ShowValidationError",
                    "Please fix the below errors to proceed",
                    attributeErrors) };
            }
            else
            {
                responseData.Data.Actions = new List<ResponseAction>() { new ResponseAction(
                "microsoft.graph.attributeCollectionSubmit.continueWithDefaultBehavior") };
            }

            return new OkObjectResult(responseData);
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

            [JsonPropertyName("message")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? Message { get; set; }

            [JsonPropertyName("attributeErrors")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public Dictionary<string, string>? AttributeErrors { get; set; }

            public ResponseAction(string dataType, string? message = null, Dictionary<string, string>? attributeErrors = null)
            {
                DataType = dataType;
                Message = message;
                AttributeErrors = attributeErrors;
            }
        }
    }
}
