using App.Bal.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace App.Bal.Repositories
{
    public class SMSService : ISMSService
    {
        private readonly IConfiguration _configuration;
        public SMSService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> SendSmsAsync(string to, string body)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(body))
                {
                    throw new ArgumentException("Both 'to' and 'body' are required.");
                }

                // Retrieve Twilio credentials from configuration
                string accountSid = _configuration["Twilio:AccountSid"];
                string authToken = _configuration["Twilio:AuthToken"];

                if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken))
                {
                    throw new Exception("Twilio configuration values are missing.");
                }

                // Initialize Twilio client
                TwilioClient.Init(accountSid, authToken);

                var message = await MessageResource.CreateAsync(
                     from: new Twilio.Types.PhoneNumber("+12764004353"),
                     body: body,
                     to: new Twilio.Types.PhoneNumber(to)
                );

                return message.Sid; // Return the message SID or confirmation
            }
            catch (Exception ex)
            {
                // Log or handle exception
                throw new Exception($"Failed to send SMS: {ex.Message}");
            }
        }
    }
}
