using App.Common.Extensions;
using App.Entity.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace VRFIDandGenetecIntegrationAPI.Controllers
{
    [SkipCustomerAuthorization]
    public class SMSController : Controller
    {
        private readonly IConfiguration _configuration;
        public SMSController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost("Twilio/SendSMS")]
        public async Task<IActionResult> SendSMS([FromBody] SendSMSRequest request)
        {
            try
            {
                // Validate the request
                if (string.IsNullOrWhiteSpace(request.To))
                {
                    return BadRequest("Recipient phone number (To) is required.");
                }
                if (string.IsNullOrWhiteSpace(request.Body))
                {
                    return BadRequest("SMS body text is required.");
                }

                // Retrieve Twilio credentials from configuration
                string accountSid = _configuration["Twilio:AccountSid"];
                string authToken = _configuration["Twilio:AuthToken"];
                string messagingServiceSid = _configuration["Twilio:MessagingServiceSid"];

                if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken) || string.IsNullOrWhiteSpace(messagingServiceSid))
                {
                    throw new Exception("Twilio configuration values are missing.");
                }

                // Twilio API URL
                string apiUrl = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";

                using var client = new HttpClient();

                // Create the request content
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("Body", request.Body),
                    new KeyValuePair<string, string>("MessagingServiceSid", messagingServiceSid),
                    new KeyValuePair<string, string>("To", request.To)
                });

                // Add Basic Authentication header
                var byteArray = System.Text.Encoding.UTF8.GetBytes($"{accountSid}:{authToken}");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                // Make the POST request
                var response = await client.PostAsync(apiUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Return success response
                var responseContent = await response.Content.ReadAsStringAsync();
                return Ok(JsonConvert.DeserializeObject(responseContent));
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("Twilio/SendSMSV2")]
        public async Task<IActionResult> SendSMSV2([FromBody] SendSMSRequest request)
        {
            try
            {
                // Validate the request
                if (string.IsNullOrWhiteSpace(request.To))
                {
                    return BadRequest("Recipient phone number (To) is required.");
                }
                if (string.IsNullOrWhiteSpace(request.Body))
                {
                    return BadRequest("SMS body text is required.");
                }

                // Retrieve Twilio credentials from configuration
                string accountSid = _configuration["Twilio:AccountSid"];
                string authToken = _configuration["Twilio:AuthToken"];

                if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken))
                {
                    throw new Exception("Twilio configuration values are missing.");
                }

                TwilioClient.Init(accountSid, authToken);

                var message = await MessageResource.CreateAsync(
                     from: new Twilio.Types.PhoneNumber("+12764004353"),
                     body: request.Body,
                     to: new Twilio.Types.PhoneNumber(request.To)
                 );

                return Ok(message.Body);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
