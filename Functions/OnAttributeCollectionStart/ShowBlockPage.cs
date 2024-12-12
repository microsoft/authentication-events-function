using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.AuthEvents.OnAttributeCollectionStart.ShowBlockPage
{
    public class ShowBlockPage
    {
        private readonly ILogger<ShowBlockPage> _logger;

        public ShowBlockPage(ILogger<ShowBlockPage> logger)
        {
            _logger = logger;
        }

        [Function("OnAttributeCollectionStart_ShowBlockPage")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // Prepare response
            ResponseObject responseData = new ResponseObject("microsoft.graph.onAttributeCollectionStartResponseData");
            responseData.Data.Actions = new List<ResponseAction>() { new ResponseAction(
                "microsoft.graph.attributeCollectionStart.showBlockPage",
                "AttributeCollectionStart Custom Extension message: Sorry, your access request has been blocked. Try reaching an admin at admin@contoso.com.") };

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
        public string Message { get; set; }

        public ResponseAction(string dataType, string messsage)
        {
            DataType = dataType;
            Message = messsage;
        }
    }
}
