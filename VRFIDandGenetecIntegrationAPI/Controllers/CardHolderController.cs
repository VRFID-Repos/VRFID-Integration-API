using App.Common.Messages;
using App.Entity.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Xml;

namespace VRFIDandGenetecIntegrationAPI.Controllers
{
    [ApiController]
    public class CardHolderController : Controller
    {
        private readonly IConfiguration _configuration;

        // Inject the configuration
        public CardHolderController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #region CardHolderAPI
        //GET:
        [HttpGet("CardHolder/GetAllCardholders")]
        public async Task<IActionResult> GetAllCardholders()
        {
            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Build the query string
            var queryParams = "EntityTypes@Cardholder";

            // Construct the request URL
            var requestUrl = $"{baseUrl}report/EntityConfiguration?q={queryParams}";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization and content type headers
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response

                try
                {
                    // Send the request and get the response
                    var response = await client.SendAsync(request);

                    // Check if the response is successful
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to retrieve cardholders.");
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

        //POST:
        [HttpPost("CardHolder/GetCardholder")]
        public async Task<IActionResult> GetCardholder([FromBody] CardholderCredentialsRequestModel entity)
        {
            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Validate cardholderId
            if (string.IsNullOrEmpty(entity.CardholderId))
            {
                return BadRequest("Cardholder ID is required.");
            }

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Build the request URL
            var requestUrl = $"{baseUrl}entity/{entity.CardholderId}";

            using (var client = new HttpClient())
            {
                // Create the request with the Authorization header
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response
                try
                {
                    // Send the request and get the response
                    var response = await client.SendAsync(request);

                    // Check if the response is successful
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to retrieve cardholder.");
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
        [HttpPost("CardHolder/CreateCardholder")]
        public async Task<IActionResult> CreateCardholder([FromBody] EntityCardholder cardholder)
        {
            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Validate cardholder object
            if (cardholder == null || string.IsNullOrEmpty(cardholder.Name) || string.IsNullOrEmpty(cardholder.FirstName) ||
                string.IsNullOrEmpty(cardholder.LastName) || string.IsNullOrEmpty(cardholder.Email) || string.IsNullOrEmpty(cardholder.MobilePhone))
            {
                return BadRequest(AppMessages.InvalidCardHolder);
            }

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Build the request URL with the cardholder data as query parameters
            var requestUrl = $"{baseUrl}entity?q=entity=NewEntity(Cardholder),Name={cardholder.Name},FirstName={cardholder.FirstName},LastName={cardholder.LastName},EmailAddress={cardholder.Email},MobilePhoneNumber={cardholder.MobilePhone},Guid";

            using (var client = new HttpClient())
            {
                // Create the request with the Authorization header
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
                        return StatusCode((int)response.StatusCode, "Failed to create cardholder");
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
        [HttpPost("CardHolder/UpdateCardholder")]
        public async Task<IActionResult> UpdateCardholder([FromBody] EntityUpdateCardholder updateModel)
        {
            if (updateModel == null || string.IsNullOrEmpty(updateModel.CardholderId))
            {
                return BadRequest("Invalid cardholder data.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Build the query string
            var queryParams = $"entity={updateModel.CardholderId}," +
                              $"FirstName={updateModel.FirstName}," +
                              $"LastName={updateModel.LastName}," +
                              $"EmailAddress={updateModel.Email}," +
                              $"MobilePhoneNumber={updateModel.MobilePhone}";

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization and content type headers
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
                        return StatusCode((int)response.StatusCode, "Failed to update cardholder.");
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
        [HttpPost("CardHolder/SetActivationDate")]
        public async Task<IActionResult> SetActivationDate([FromBody] EntityActivationDate model)
        {
            if (model == null || string.IsNullOrEmpty(model.CardholderId))
            {
                return BadRequest("Invalid cardholder data.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Format dates for query string (API likely expects date format like yyyy-MM-dd)
            var activationDate = model.ActivationDate.ToString("yyyy-MM-dd");
            var deactivationDate = model.DeactivationDate.HasValue
                ? model.DeactivationDate.Value.ToString("yyyy-MM-dd")
                : "";

            // Build the query string
            var queryParams = $"entity={model.CardholderId},Status.Activate({activationDate},{deactivationDate})";

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization and content type headers
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
                        return StatusCode((int)response.StatusCode, "Failed to set activation date.");
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
        [HttpPost("CardHolder/ActivateCardholderAtDate")]
        public async Task<IActionResult> ActivateCardholderAtDate([FromBody] CardholderActivationDateRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Cardholder))
            {
                return BadRequest("Cardholder GUID is required.");
            }

            if (model.ActivationDate == default)
            {
                return BadRequest("Activation date is required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Format and URL encode the activation date
            var activationDateString = model.ActivationDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var encodedCardholder = HttpUtility.UrlEncode($"entity={model.Cardholder}");
            var encodedActivationDate = HttpUtility.UrlEncode($"Status.Activate({activationDateString})");

            // Build the query string
            var queryParams = $"{encodedCardholder}%2C{encodedActivationDate}"; // '%2C' is the URL encoding for ','

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization and content type headers
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
                        return StatusCode((int)response.StatusCode, "Failed to activate cardholder at the specified date.");
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
        [HttpPost("CardHolder/DeactivateCardholderAtDate")]
        public async Task<IActionResult> DeactivateCardholderAtDate([FromBody] CardholderDeactivationAtDateRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Cardholder))
            {
                return BadRequest("Cardholder GUID is required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the cardholder GUID and the expiration date
            var encodedCardholder = HttpUtility.UrlEncode($"entity={model.Cardholder}");
            var encodedExpirationDate = HttpUtility.UrlEncode($"Status.Deactivate({model.ExpirationDate:yyyy-MM-ddTHH:mm:ssZ})");

            // Build the query string
            var queryParams = $"{encodedCardholder}%2C{encodedExpirationDate}"; // '%2C' is the URL encoding for ','

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization and content type headers
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
                        return StatusCode((int)response.StatusCode, "Failed to deactivate the cardholder.");
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
        [HttpPost("CardHolder/GetCardholderCredentials")]
        public async Task<IActionResult> GetCardholderCredentials([FromBody] CardholderCredentialsRequestModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.CardholderId))
            {
                return BadRequest("Invalid cardholder data.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Build the query string
            var queryParams = $"entity={model.CardholderId},Credentials";

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization and content type headers
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
                        return StatusCode((int)response.StatusCode, "Failed to retrieve cardholder credentials.");
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
        [HttpPost("CardHolder/ActivateCardholder")]
        public async Task<IActionResult> ActivateCardholder([FromBody] CardholderCredentialsRequestModel model)
        {
            if (string.IsNullOrEmpty(model.CardholderId))
            {
                return BadRequest("Cardholder GUID is required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the cardholder GUID and build the query string
            var encodedCardholder = HttpUtility.UrlEncode($"entity={model.CardholderId}");
            var queryParams = $"{encodedCardholder}%2CStatus.Activate()"; // '%2C' is the URL encoding for ','

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization and content type headers
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
                        return StatusCode((int)response.StatusCode, "Failed to activate cardholder.");
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
        [HttpPost("CardHolder/DeactivateCardholder")]
        public async Task<IActionResult> DeactivateCardholder([FromBody] CardholderCredentialsRequestModel model)
        {
            if (string.IsNullOrEmpty(model.CardholderId))
            {
                return BadRequest("Cardholder GUID is required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the cardholder GUID
            var encodedCardholder = HttpUtility.UrlEncode($"entity={model.CardholderId}");
            var deactivateStatus = HttpUtility.UrlEncode("Status.Deactivate()");

            // Build the query string
            var queryParams = $"{encodedCardholder}%2C{deactivateStatus}"; // '%2C' is the URL encoding for ','

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization and content type headers
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
                        return StatusCode((int)response.StatusCode, "Failed to deactivate the cardholder.");
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
        [HttpPost("CardHolder/SetActivationAndExpirationDates")]
        public async Task<IActionResult> SetActivationAndExpirationDates([FromBody] CardholderActivationWithExpirationRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Cardholder))
            {
                return BadRequest("Cardholder GUID is required.");
            }

            if (model.ActivationDate == default)
            {
                return BadRequest("Activation date is required.");
            }

            if (model.ExpirationDate == default)
            {
                return BadRequest("Expiration date is required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Format and URL encode the dates
            var activationDateString = model.ActivationDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var expirationDateString = model.ExpirationDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var encodedCardholder = HttpUtility.UrlEncode($"entity={model.Cardholder}");
            var encodedDates = HttpUtility.UrlEncode($"Status.Activate({activationDateString},{expirationDateString})");

            // Build the query string
            var queryParams = $"{encodedCardholder}%2C{encodedDates}"; // '%2C' is the URL encoding for ','

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization and content type headers
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
                        return StatusCode((int)response.StatusCode, "Failed to set activation and expiration dates for the cardholder.");
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
        [HttpPost("CardHolder/AssignCardholderToGroup")]
        public async Task<IActionResult> AssignCardholderToGroup([FromBody] CardholderGroupAssignmentRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Group) || string.IsNullOrEmpty(model.Member))
            {
                return BadRequest("Group and Member GUIDs are required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the group GUID and the member GUID
            var encodedGroup = HttpUtility.UrlEncode($"entity={model.Group}");
            var encodedMember = HttpUtility.UrlEncode($"Members@{model.Member}");

            // Build the query string
            var queryParams = $"{encodedGroup}%2C{encodedMember}"; // '%2C' is the URL encoding for ','

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization and content type headers
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
                        return StatusCode((int)response.StatusCode, "Failed to assign cardholder to group.");
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
        [HttpPost("CardHolder/UnassignCardholderFromGroup")]
        public async Task<IActionResult> UnassignCardholderFromGroup([FromBody] CardholderGroupAssignmentRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Group) || string.IsNullOrEmpty(model.Member))
            {
                return BadRequest("Group and Cardholder GUIDs are required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the group GUID and the cardholder GUID for unassignment
            var encodedGroup = HttpUtility.UrlEncode($"entity={model.Group}");
            var encodedCardholder = HttpUtility.UrlEncode($"Members-({model.Member})");

            // Build the query string
            var queryParams = $"{encodedGroup}%2C{encodedCardholder}"; // '%2C' is the URL encoding for ','

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization and content type headers
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
                        return StatusCode((int)response.StatusCode, "Failed to unassign cardholder from group.");
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
        [HttpPost("CardHolder/CanSetCardholderPicture")]
        public async Task<IActionResult> CanSetCardholderPicture([FromBody] CardholderPictureRequestModel2 model)
        {
            if (string.IsNullOrEmpty(model.Cardholder))
            {
                return BadRequest("Cardholder GUID is required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the cardholder GUID
            var encodedCardholder = HttpUtility.UrlEncode($"entity={model.Cardholder}");
            var encodedFunction = HttpUtility.UrlEncode($"CanSetPicture()");

            // Construct the query string
            var queryParams = $"{encodedCardholder}%2C{encodedFunction}"; // '%2C' is the URL encoding for ','

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
                        return StatusCode((int)response.StatusCode, "Failed to check if setting picture is possible.");
                    }

                    // Read the response content
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Return the JSON response directly
                    return Content(responseBody, "application/json" ); // Convert XML to JSON if necessary
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpPost("CardHolder/AssignCredentialToCardholder")]
        public async Task<IActionResult> AssignCredentialToCardholder([FromBody] CredentialAssignmentRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Cardholder) || string.IsNullOrEmpty(model.Credential))
            {
                return BadRequest("Both Cardholder GUID and Credential are required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the cardholder GUID and credential
            var encodedCardholder = HttpUtility.UrlEncode($"entity={model.Cardholder}");
            var encodedCredential = HttpUtility.UrlEncode($"Credentials@{model.Credential}");

            // Construct the query string
            var queryParams = $"{encodedCardholder}%2C{encodedCredential}"; // '%2C' is the URL encoding for ','

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
                        return StatusCode((int)response.StatusCode, "Failed to assign credential to cardholder.");
                    }

                    // Read the response content
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Return the JSON response directly
                    return Content(responseBody, "application/json"); // Convert XML to JSON if necessary
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }


        [HttpPost("CardHolder/CreateCardholderFromNewBook")]
        public async Task<IActionResult> CreateCardholderFromNewBook([FromBody] EntityCardholder cardholder)
        {
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            if (string.IsNullOrEmpty(cardholder.Name))
            {
                return BadRequest(AppMessages.InvalidCardHolder);
            }

            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);
            var requestUrl = $"{baseUrl}entity?q=entity=NewEntity(Cardholder),Name={cardholder.Name},FirstName={cardholder.FirstName},LastName={cardholder.LastName},EmailAddress={cardholder.Email},MobilePhoneNumber={cardholder.MobilePhone},Guid";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json"));

                try
                {
                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to create cardholder");
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var cardholderResponse = JsonConvert.DeserializeObject<ResponseModel>(responseBody);

                    return Ok(cardholderResponse); // Return the parsed JSON response directly
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpPost("CardHolder/SetActivationAndExpirationDatesForNewBook")]
        public async Task<IActionResult> SetActivationAndExpirationDatesForNewBook([FromBody] CardholderActivationWithExpirationRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Cardholder))
            {
                return BadRequest("Cardholder GUID is required.");
            }

            if (model.ActivationDate == default)
            {
                return BadRequest("Activation date is required.");
            }

            if (model.ExpirationDate == default)
            {
                return BadRequest("Expiration date is required.");
            }

            // Get API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Format and URL encode the dates
            var activationDateString = model.ActivationDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var expirationDateString = model.ExpirationDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var encodedCardholder = HttpUtility.UrlEncode($"entity={model.Cardholder}");
            var encodedDates = HttpUtility.UrlEncode($"Status.Activate({activationDateString},{expirationDateString})");

            // Build the query string
            var queryParams = $"{encodedCardholder}%2C{encodedDates}"; // '%2C' is the URL encoding for ','

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

            using (var client = new HttpClient())
            {
                // Create the request with Authorization and content type headers
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
                        return StatusCode((int)response.StatusCode, "Failed to set activation and expiration dates for the cardholder.");
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
        #endregion
    }
}
