using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.AuthEvents.OnAttributeCollectionStart.SetPrefillValues
{
    public class SetPrefillValues
    {
        private readonly ILogger<SetPrefillValues> _logger;

        public SetPrefillValues(ILogger<SetPrefillValues> logger)
        {
            _logger = logger;
        }

        [Function("OnAttributeCollectionStart_SetPrefillValues")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // The form fields will load with these default values
            var inputs = new Dictionary<string, object>()
            {
                { "postalCode", "<your-prefill-value>" },
                { "streetAddress", "<your-prefill-value>" },
                { "city", "<your-prefill-value>" },
                { "extension_appId_mailingList", false },
                { "extension_appId_memberSince", 2023 }
            };

            // Prepare response
            ResponseObject responseData = new ResponseObject("microsoft.graph.onAttributeCollectionStartResponseData");
            responseData.Data.Actions = new List<ResponseAction>() { new ResponseAction(
                "microsoft.graph.attributeCollectionStart.setPrefillValues",
                inputs) };

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

        [JsonPropertyName("inputs")]
        public Dictionary<string, object> Inputs { get; set; }

        public ResponseAction(string dataType, Dictionary<string, object> inputs)
        {
            DataType = dataType;
            Inputs = inputs;
        }
    }
}
