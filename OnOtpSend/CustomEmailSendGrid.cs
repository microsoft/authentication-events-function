using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Azure.Communication.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.AuthEvents.OnOtpSend.CustomEmailSendGrid
{
    public class CustomEmailSendGrid
    {
        private readonly ILogger<CustomEmailSendGrid> _logger;

        public CustomEmailSendGrid(ILogger<CustomEmailSendGrid> logger)
        {
            _logger = logger;
        }

        [Function("OnOtpSend_CustomEmailSendGrid")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            // Get the request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            JsonNode jsonPayload = JsonNode.Parse(requestBody)!;

            // Get OTP and mail to
            string emailTo = jsonPayload["data"]!["otpContext"]!["identifier"]!.ToString();
            string otp = jsonPayload["data"]!["otpContext"]!["onetimecode"]!.ToString();

            // Send email
            await SendEmailAsync(emailTo, otp);

            // Prepare response
            ResponseObject responseData = new ResponseObject("microsoft.graph.OnOtpSendResponseData");
            responseData.Data.Actions = new List<ResponseAction>() { new ResponseAction(
                "microsoft.graph.OtpSend.continueWithDefaultBehavior") };

            return new OkObjectResult(responseData);
        }

        private async Task SendEmailAsync(string emailTo, string code)
        {
            // Get app settings
            var apiKey = Environment.GetEnvironmentVariable("mail_sendgridKey");
            var sender = Environment.GetEnvironmentVariable("mail_sender");
            var senderName = Environment.GetEnvironmentVariable("mail_senderName");
            var template = Environment.GetEnvironmentVariable("mail_template");

            _logger.LogInformation($"Sending OTP to {emailTo}");

            try
            {
                if (!string.IsNullOrEmpty(apiKey))
                {
                    var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send");
                    request.Headers.Add("Authorization", $"Bearer {apiKey}");

                    SendGridMessage msg = new SendGridMessage()
                    {
                        template_id = template,
                        from = new Person { email = sender, name = senderName },
                        personalizations = new List<Personalization> {

                new Personalization(emailTo, code)
                }
                    };

                    request.Content = new StringContent(msg.ToString(), null, "application/json");

                    var response = await client.SendAsync(request);
                    _logger.LogInformation($"Sendgrid response: {response.StatusCode}");
                    response.EnsureSuccessStatusCode();
                    _logger.LogInformation(await response.Content.ReadAsStringAsync());
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message);
            }
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

        public ResponseAction(string dataType)
        {
            DataType = dataType;
        }
    }

    public class EmailTemplate
    {
        public static string GenerateBody(string oneTimeCode)
        {
            return @$"<html><body>
            <div style='background-color: #1F6402!important; padding: 15px'>
                <table>
                <tbody>
                    <tr>
                        <td colspan='2' style='padding: 0px;font-family: &quot;Segoe UI Semibold&quot;, &quot;Segoe UI Bold&quot;, &quot;Segoe UI&quot;, &quot;Helvetica Neue Medium&quot;, Arial, sans-serif;font-size: 17px;color: white;'>Woodgrove Groceries live demo</td>
                    </tr>
                    <tr>
                        <td colspan='2' style='padding: 15px 0px 0px;font-family: &quot;Segoe UI Light&quot;, &quot;Segoe UI&quot;, &quot;Helvetica Neue Medium&quot;, Arial, sans-serif;font-size: 35px;color: white;'>Your Woodgrove verification code</td>
                    </tr>
                    <tr>
                        <td colspan='2' style='padding: 25px 0px 0px;font-family: &quot;Segoe UI&quot;, Tahoma, Verdana, Arial, sans-serif;font-size: 14px;color: white;'> To access <span style='font-family: &quot;Segoe UI Bold&quot;, &quot;Segoe UI Semibold&quot;, &quot;Segoe UI&quot;, &quot;Helvetica Neue Medium&quot;, Arial, sans-serif; font-size: 14px; font-weight: bold; color: white;'>Woodgrove Groceries</span>'s app, please copy and enter the code below into the sign-up or sign-in page. This code is valid for 30 minutes. </td>
                    </tr>
                    <tr>
                        <td colspan='2' style='padding: 25px 0px 0px;font-family: &quot;Segoe UI&quot;, Tahoma, Verdana, Arial, sans-serif;font-size: 14px;color: white;'>Your account verification code:</td>
                    </tr>
                    <tr>
                        <td style='padding: 0px;font-family: &quot;Segoe UI Bold&quot;, &quot;Segoe UI Semibold&quot;, &quot;Segoe UI&quot;, &quot;Helvetica Neue Medium&quot;, Arial, sans-serif;font-size: 25px;font-weight: bold;color: white;padding-top: 5px;'>
                        {oneTimeCode}</td>
                        <td rowspan='3' style='text-align: center;'>
                            <img src='https://woodgrovedemo.com/custom-email/shopping.png' style='border-radius: 50%; width: 100px'>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 25px 0px 0px;font-family: &quot;Segoe UI&quot;, Tahoma, Verdana, Arial, sans-serif;font-size: 14px;color: white;'> If you didn't request a code, you can ignore this email. </td>
                    </tr>
                    <tr>
                        <td style='padding: 25px 0px 0px;font-family: &quot;Segoe UI&quot;, Tahoma, Verdana, Arial, sans-serif;font-size: 14px;color: white;'> Best regards, </td>
                    </tr>
                    <tr>
                        <td>
                            <img src='https://woodgrovedemo.com/Company-branding/headerlogo.png' height='20'>
                        </td>
                        <td style='font-family: &quot;Segoe UI&quot;, Tahoma, Verdana, Arial, sans-serif;font-size: 14px;color: white; text-align: center;'>
                            <a href='https://woodgrovedemo.com/Privacy' style='color: white; text-decoration: none;'>Privacy Statement</a>
                        </td>
                    </tr>
                </tbody>
                </table>
            </div>
            </body></html>";
        }
    }

    /********* SendGrid data ********/
    public class SendGridMessage
    {
        public List<Personalization> personalizations { get; set; }
        public string template_id { get; set; }
        public Person from { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    public class Personalization
    {
        public Personalization(string to, string otp)
        {
            this.to = new List<Person>() { new Person() { email = to } };
            this.dynamic_template_data = new DynamicTemplateData() { otp = otp };
        }

        public List<Person> to { get; set; }
        public DynamicTemplateData dynamic_template_data { get; set; }
    }

    public class DynamicTemplateData
    {
        public string otp { get; set; }
    }

    public class Person
    {
        public string email { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string name { get; set; }
    }

    public class OTPCodeTemplateData
    {
        [JsonPropertyName("otp")]
        public string OTPCode { get; set; }

    }
}

