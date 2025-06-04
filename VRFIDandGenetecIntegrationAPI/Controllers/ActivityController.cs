using App.Common.Model;
using App.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace VRFIDandGenetecIntegrationAPI.Controllers
{
    public class ActivityController : Controller
    {
        private readonly IConfiguration _configuration;

        // Inject the configuration
        public ActivityController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // GET: Cardholder Activity
        [HttpGet("Activity/GetCardholderActivity")]
        public async Task<IActionResult> GetCardholderActivity([FromQuery] string cardholders)
        {
            // Validate the input
            if (string.IsNullOrWhiteSpace(cardholders))
            {
                return BadRequest("Cardholder identifiers are required.");
            }

            try
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

                // Encode the credentials for Basic Auth
                var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

                // Build the query string
                var queryParams = $"Cardholders@{cardholders}";

                // Construct the request URL
                var requestUrl = $"{baseUrl}report/CardholderActivity?q={Uri.EscapeDataString(queryParams)}";

                using (var client = new HttpClient())
                {
                    // Create the request with Authorization and content type headers
                    var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response

                    // Send the request and get the response
                    var response = await client.SendAsync(request);

                    // Check if the response is successful
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to retrieve cardholder activity.");
                    }

                    // Read the response content
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Return the JSON response directly
                    return Content(responseBody, "application/json");
                }
            }
            catch (HttpRequestException e)
            {
                // Return an error response in case of exception
                return StatusCode(500, $"Request error: {e.Message}");
            }
        }
        [HttpGet("Activity/GetCardholderActivityV2")]
        public async Task<IActionResult> GetCardholderActivity(
        [FromQuery] string cardholders,
        [FromQuery] string events = null,
        [FromQuery] string customEvents = null,
        [FromQuery] string entities = null,
        [FromQuery] string startTime = null,
        [FromQuery] string endTime = null,
        [FromQuery] string sortOrder = "ascending",
        [FromQuery] int maximumResultCount = 100)
        {
            try
            {
                // Validate required parameter
                if (string.IsNullOrWhiteSpace(cardholders))
                {
                    return BadRequest("Cardholders parameter is required.");
                }

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

                // Encode the credentials for Basic Auth
                var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

                // Construct the query parameters
                var queryParameters = new List<string> {
                $"Cardholders@{cardholders}",
                "Events*",
                string.IsNullOrWhiteSpace(events) ? null : $"Events@{events}",
                string.IsNullOrWhiteSpace(customEvents) ? null : $"CustomEvents@{customEvents}",
                string.IsNullOrWhiteSpace(entities) ? null : $"ExcludedExpansionEntities@{entities}",
                string.IsNullOrWhiteSpace(startTime) || string.IsNullOrWhiteSpace(endTime) ? null : $"TimeRange.SetTimeRange({startTime},{endTime})",
                $"SortOrder={sortOrder}",
                $"MaximumResultCount={maximumResultCount}"
            };

                // Filter out null or empty query parts
                queryParameters.RemoveAll(param => string.IsNullOrWhiteSpace(param));

                // Construct the request URL
                string queryString = string.Join(",", queryParameters);
                string requestUrl = $"{baseUrl}report/CardholderActivity?q={queryString}";

                using var client = new HttpClient();

                // Create the request
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json"));

                // Send the request and get the response
                var response = await client.SendAsync(request);

                // Check if the response is successful
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve cardholder activities.");
                }

                // Read the response content
                var responseBody = await response.Content.ReadAsStringAsync();

                // Return the JSON response directly
                return Content(responseBody, "application/json");
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // GET: Credential Activity
        [HttpGet("Activity/GetCredentialActivity")]
        public async Task<IActionResult> GetCredentialActivityByCredentials([FromQuery] string credentials)
        {
            if (string.IsNullOrWhiteSpace(credentials))
            {
                return BadRequest("Credentials parameter is required.");
            }

            try
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

                // Encode the credentials for Basic Auth
                var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

                // Construct the API request URL
                var requestUrl = $"{baseUrl}report/CredentialActivity?q=Credentials@{credentials}";

                using var client = new HttpClient();

                // Create the request
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json"));

                // Send the request
                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, $"Failed to retrieve credential activity for: {credentials}.");
                }

                // Parse the response body
                var responseBody = await response.Content.ReadAsStringAsync();

                // Return the response
                return Content(responseBody, "application/json");
            }
            catch (Exception ex)
            {
                // Log the exception and return an error
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("Activity/GetCredentialActivityV2")]
        public async Task<IActionResult> GetCredentialActivity(
        [FromQuery] string credentials,
        [FromQuery] string events = null,
        [FromQuery] string customEvents = null,
        [FromQuery] string entities = null,
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null,
        [FromQuery] string sortOrder = "ascending",
        [FromQuery] int? maximumResultCount = 100)
        {
            // Validate the required parameters
            if (string.IsNullOrWhiteSpace(credentials))
            {
                return BadRequest("At least one credential GUID or logical ID is required.");
            }

            try
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
                var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

                // Build the query string parameters
                var queryStringParameters = new List<string>
                {
                    $"Credentials@{credentials}",
                    "Events*"
                };

                if (!string.IsNullOrWhiteSpace(events))
                {
                    queryStringParameters.Add($"Events@{events}");
                }

                if (!string.IsNullOrWhiteSpace(customEvents))
                {
                    queryStringParameters.Add($"CustomEvents@{customEvents}");
                }

                if (!string.IsNullOrWhiteSpace(entities))
                {
                    queryStringParameters.Add($"ExcludedExpansionEntities@{entities}");
                }

                if (startTime.HasValue && endTime.HasValue)
                {
                    queryStringParameters.Add($"TimeRange.SetTimeRange({startTime.Value:o},{endTime.Value:o})");
                }

                if (!string.IsNullOrWhiteSpace(sortOrder))
                {
                    queryStringParameters.Add($"SortOrder={sortOrder}");
                }

                if (maximumResultCount.HasValue)
                {
                    queryStringParameters.Add($"MaximumResultCount={maximumResultCount.Value}");
                }

                var queryString = string.Join(",", queryStringParameters);
                var requestUrl = $"{baseUrl}report/CredentialActivity?q={Uri.EscapeDataString(queryString)}";

                using var client = new HttpClient();

                // Create the HTTP GET request with authorization header
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json"));

                // Send the request
                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve credential activity.");
                }

                // Read the response content
                var responseBody = await response.Content.ReadAsStringAsync();

                // Return the JSON response
                return Content(responseBody, "application/json");
            }
            catch (Exception ex)
            {
                // Handle and log any exceptions
                Console.WriteLine($"Exception occurred: {ex.Message}");
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }

    }
}
