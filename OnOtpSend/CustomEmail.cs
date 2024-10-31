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

namespace Company.Function
{
    public class CustomEmail
    {
        private readonly ILogger<CustomEmail> _logger;

        public CustomEmail(ILogger<CustomEmail> logger)
        {
            _logger = logger;
        }

        [Function(nameof(CustomEmail))]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
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
            ResponseData responseData = ResponseData.GenerateResponse("microsoft.graph.OtpSend.continueWithDefaultBehavior");
            return new OkObjectResult(responseData);
        }

        private async Task SendEmailAsync(string emailTo, string code)
        {
            // Get app settings
            var connectionString = Environment.GetEnvironmentVariable("mail_connectionString");
            var sender = Environment.GetEnvironmentVariable("mail_sender");
            var subject = Environment.GetEnvironmentVariable("mail_subject");

            var emailClient = new EmailClient(connectionString);
            var body = EmailTemplate.GenerateBody(code);

            _logger.LogInformation($"Sending OTP to {emailTo}");

            try
            {
                EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                Azure.WaitUntil.Started,
                sender,
                emailTo,
                subject,
                body);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
    public class ResponseData
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }

        public static ResponseData GenerateResponse(string actionodatattype, string responseodatattype = "microsoft.graph.OnOtpSendResponseData")
        {
            return new ResponseData()
            {
                Data = new Data(responseodatattype)
                {
                    actions = new List<Action>()
                        {
                            new Action(actionodatattype)
                        }
                }
            };
        }
    }

    public class Data
    {
        [JsonPropertyName("@odata.type")]
        public string odatatype { get; set; }
        public List<Action> actions { get; set; }

        public Data(string responseodatattype)
        {
            odatatype = responseodatattype;
        }
    }

    public class Action
    {
        [JsonPropertyName("@odata.type")]
        public string odatatype { get; set; }

        public Action(string newodatattype, string mesg = null)
        {
            odatatype = newodatattype;
        }
    }

    public class SendGridMessage
    {
        public List<Personalization> personalizations { get; set; }
        public string template_id { get; set; }
        public Person from { get; set; }
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
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string email { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string name { get; set; }
    }

    public class OTPCodeTemplateData
    {
        [JsonPropertyName("otp")]
        public string OTPCode { get; set; }
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
}

