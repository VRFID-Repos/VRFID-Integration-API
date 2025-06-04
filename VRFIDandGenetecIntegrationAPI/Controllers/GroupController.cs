using App.Entity.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Web;

namespace VRFIDandGenetecIntegrationAPI.Controllers
{
    [ApiController]
    public class GroupController : Controller
    {
        private readonly IConfiguration _configuration;

        // Inject the configuration
        public GroupController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #region CardHolderGroupAPI
        //GET:
        [HttpGet("CardholderGroup/GetCardholderGroups")]
        public async Task<IActionResult> GetCardholderGroups()
        {
            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Construct the request URL with the encoded query parameter
            var requestUrl = $"{baseUrl}report/EntityConfiguration?q=EntityTypes%40CardholderGroup";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization and content type headers
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json"));

                try
                {
                    // Send the request and get the response
                    var response = await client.SendAsync(request);

                    // Check if the response is successful
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to retrieve cardholder groups.");
                    }

                    // Read the response content
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Return the JSON response directly
                    return Content(responseBody, "application/json");
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpGet("CardholderGroup/GetCardholderGroupMembers")]
        public async Task<IActionResult> GetCardholderGroupMembers(string group)
        {
            if (string.IsNullOrEmpty(group))
            {
                return BadRequest("Group identifier is required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the group parameter
            var encodedGroup = HttpUtility.UrlEncode($"entity={group},Members");

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={encodedGroup}";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization and Accept headers
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Set to text/json to receive JSON response

                try
                {
                    // Send the request and get the response
                    var response = await client.SendAsync(request);

                    // Check if the response is successful
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to retrieve cardholder group members.");
                    }

                    // Read and return the JSON response directly
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return Content(responseBody, "application/json");
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        
        //POST:
        [HttpPost("CardholderGroup/CreateCardholderGroup")]
        public async Task<IActionResult> CreateCardholderGroup([FromBody] EntityCardHolderGroup model)
        {
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.Email))
            {
                return BadRequest("Name and EmailAddress are required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the parameters
            var encodedName = HttpUtility.UrlEncode($"Name={model.Name}");
            var encodedEmail = HttpUtility.UrlEncode($"EmailAddress={model.Email}");
            var encodedGuid = HttpUtility.UrlEncode("Guid"); // Guid as a constant string

            // Construct the query string
            var queryParams = $"entity=NewEntity(CardholderGroup),{encodedName},{encodedEmail},{encodedGuid}";

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization header
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response

                try
                {
                    // Send the request and get the response
                    var response = await client.SendAsync(request);

                    // Check if the response is successful
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to create cardholder group.");
                    }

                    // Read the response content
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Return the JSON response directly
                    return Content(responseBody, "application/json");
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpPost("CardholderGroup/ChangeAccessPermissionLevel")]
        public async Task<IActionResult> ChangeAccessPermissionLevel([FromBody] EntityCardHolderGroupLevel model)
        {
            if (string.IsNullOrEmpty(model.Group) || string.IsNullOrEmpty(model.Level))
            {
                return BadRequest("Group identifier and permission level are required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the group and level parameters
            var encodedGroup = HttpUtility.UrlEncode($"entity={model.Group}");
            var encodedLevel = HttpUtility.UrlEncode($"SetAccessPermissionLevel({model.Level})");

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={encodedGroup}%2C{encodedLevel}"; // '%2C' is the URL encoding for ','

            using (var client = new HttpClient())
            {
                // Create the request with Authorization header
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response

                try
                {
                    // Send the request and get the response
                    var response = await client.SendAsync(request);

                    // Check if the response is successful
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to change access permission level for the cardholder group.");
                    }

                    // Read the response content
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Return the JSON response directly
                    return Content(responseBody, "application/json");
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpPost("CardholderGroup/AddCardholderToGroup")]
        public async Task<IActionResult> AddCardholderToGroup([FromBody] CardholderGroupRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Group) || string.IsNullOrEmpty(model.Cardholder))
            {
                return BadRequest("Group and Cardholder identifiers are required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the group and cardholder parameters
            var encodedGroup = HttpUtility.UrlEncode($"entity={model.Group}");
            var encodedCardholder = HttpUtility.UrlEncode($"AddCardholderIntoGroup({model.Cardholder})");

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={encodedGroup}%2C{encodedCardholder}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response

                try
                {
                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to add cardholder to the group.");
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();

                    return Ok(responseBody);
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpPost("CardholderGroup/RemoveCardholderFromGroup")]
        public async Task<IActionResult> RemoveCardholderFromGroup([FromBody] CardholderGroupRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Group) || string.IsNullOrEmpty(model.Cardholder))
            {
                return BadRequest("Both Group and Cardholder identifiers are required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the group and cardholder parameters
            var encodedGroup = HttpUtility.UrlEncode($"entity={model.Group}");
            var encodedCardholder = HttpUtility.UrlEncode($"RemoveCardholderFromGroup({model.Cardholder})");

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={encodedGroup}%2C{encodedCardholder}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response

                try
                {
                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to remove cardholder from the group.");
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();

                    return Ok(responseBody);
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpPost("CardholderGroup/MoveCardholderToGroup")]
        public async Task<IActionResult> MoveCardholderToGroup([FromBody] CardholderGroupRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Cardholder) || string.IsNullOrEmpty(model.Group))
            {
                return BadRequest("Both Cardholder and Group identifiers are required.");
            }

            // Retrieve API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the cardholder and group parameters
            var encodedCardholder = HttpUtility.UrlEncode($"entity={model.Cardholder}");
            var encodedGroup = HttpUtility.UrlEncode($"Groups*@{model.Group}");

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={encodedCardholder}%2C{encodedGroup}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response

                try
                {
                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to move the cardholder to the new group.");
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();

                    return Ok(responseBody);
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }

        [HttpPost("CardholderGroup/CreateCardholderGroupForNewBook")]
        public async Task<IActionResult> CreateCardholderGroupForNewBook([FromBody] EntityCardHolderGroup model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                return BadRequest("Name is required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the parameters
            var encodedName = HttpUtility.UrlEncode($"Name={model.Name}");
            var encodedEmail = HttpUtility.UrlEncode($"EmailAddress={model.Email}");
            var encodedGuid = HttpUtility.UrlEncode("Guid"); // Guid as a constant string

            // Construct the query string
            var queryParams = $"entity=NewEntity(CardholderGroup),{encodedName},{encodedEmail},{encodedGuid}";

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization header
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response

                try
                {
                    // Send the request and get the response
                    var response = await client.SendAsync(request);

                    // Check if the response is successful
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to create cardholder group.");
                    }

                    // Read the response content
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Return the JSON response directly
                    return Ok(responseBody);
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpPost("CardholderGroup/AddCardholderToGroupForNewbook")]
        public async Task<IActionResult> AddCardholderToGroupForNewbook([FromBody] CardholderGroupRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Group) || string.IsNullOrEmpty(model.Cardholder))
            {
                return BadRequest("Group and Cardholder identifiers are required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the group and cardholder parameters
            var encodedGroup = HttpUtility.UrlEncode($"entity={model.Group}");
            var encodedCardholder = HttpUtility.UrlEncode($"AddCardholderIntoGroup({model.Cardholder})");

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={encodedGroup}%2C{encodedCardholder}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response

                try
                {
                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to add cardholder to the group.");
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();

                    return Ok(responseBody);
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
