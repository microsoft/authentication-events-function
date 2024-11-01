using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.AuthEvents.OnAttributeCollectionSubmit.ModifyAttributeValues
{
    public class ModifyAttributeValues
    {
        private readonly ILogger<ModifyAttributeValues> _logger;

        public ModifyAttributeValues(ILogger<ModifyAttributeValues> logger)
        {
            _logger = logger;
        }

        [Function("OnAttributeCollectionSubmit_ModifyAttributeValues")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // User attributes will be saved with these override values
            var attributes = new Dictionary<string, object>()
                {
                    { "postalCode", "<your-override-value>" },
                    { "streetAddress", "<your-override-value>" },
                    { "city", "<your-override-value>" },
                    { "extension_appId_mailingList", false },
                    { "extension_appId_memberSince", 2010 }
                };

            // Prepare response
            ResponseObject responseData = new ResponseObject("microsoft.graph.onAttributeCollectionSubmitResponseData");
            responseData.Data.Actions = new List<ResponseAction>() { new ResponseAction(
                "microsoft.graph.attributeCollectionSubmit.modifyAttributeValues",
                attributes) };

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

        [JsonPropertyName("attributes")]
        public Dictionary<string, object> Attributes { get; set; }

        public ResponseAction(string dataType, Dictionary<string, object> attributes)
        {
            DataType = dataType;
            Attributes = attributes;
        }
    }
}
