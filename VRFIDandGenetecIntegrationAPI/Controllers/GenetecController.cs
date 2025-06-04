using App.Common;
using App.Common.Extensions;
using App.Common.Messages;
using App.Common.Model;
using App.Entity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace VRFIDandGenetecIntegrationAPI.Controllers
{
    [ApiController]
    public class GenetecController : Controller
    {
        private readonly IConfiguration _configuration;

        // Inject the configuration
        public GenetecController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
       
        #region GeneralAPIs
        [HttpGet("General/GetOnlineStatus")]
        public async Task<IActionResult> GetOnlineStatus()
        {
            // Retrieve customer details from HttpContext
            var customer = HttpContext.Items["CustomerDetails"] as CustomerDTO;

            if (customer == null)
            {
                return Unauthorized("Customer details not found.");
            }
            // Get NewBook credentials
            var genetecCredentials = CredentialHelper.GetGenetecCredentials(customer);

            if (genetecCredentials == null)
                return BadRequest("Genetec credentials not found or integration not enabled.");

            // Get API credentials and base URL
            var (username, password, baseUrl) = genetecCredentials.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(baseUrl))
            {
                return BadRequest(AppMessages.MissingCredentials);
            }

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            using (var client = new HttpClient())
            {
                // Create the request with the Authorization header
                var request = new HttpRequestMessage(HttpMethod.Get, baseUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response

                try
                {
                    // Send the request and get the response
                    var response = await client.SendAsync(request);

                    // Check if the response is successful
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, AppMessages.FailedToFetch);
                    }

                    // Read the response content
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Return the response as JSON
                    return Content(responseBody, "application/json");
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        #endregion
    }
}
