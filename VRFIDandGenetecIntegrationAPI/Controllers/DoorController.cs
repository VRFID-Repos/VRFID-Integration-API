using App.Common.Model;
using App.Common;
using App.Entity.Models;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace VRFIDandGenetecIntegrationAPI.Controllers
{
    [ApiController]
    public class DoorController : Controller
    {
        private readonly IConfiguration _configuration;

        // Inject the configuration
        public DoorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #region DoorAPI
        //GET:
        [HttpGet("Door/GetDoor/{door}")]
        public async Task<IActionResult> GetDoor(string door)
        {
            if (string.IsNullOrEmpty(door))
            {
                return BadRequest("Door ID is required.");
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

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity/{door}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response

                try
                {
                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to retrieve door properties.");
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
        [HttpGet("Door/GetDoors")]
        public async Task<IActionResult> GetDoors()
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

            // Construct the request URL
            var requestUrl = $"{baseUrl}report/EntityConfiguration?q=EntityTypes@Door";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response

                try
                {
                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to retrieve doors.");
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
        [HttpGet("Door/GetAccessRulesOnDoor/{door}")]
        public async Task<IActionResult> GetAccessRulesOnDoor(string door)
        {
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Build the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(door)},AccessRules";

            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Specify JSON response format

                try
                {
                    var response = await client.SendAsync(httpRequest);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(responseBody);
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, responseBody);
                    }
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpGet("Door/GetAreas")]
        public async Task<IActionResult> GetAreas([FromQuery] string door)
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

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(door)},Areas";

            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response

                try
                {
                    var response = await client.SendAsync(httpRequest);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(responseBody);
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, responseBody);
                    }
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }



        //POST:
        [HttpPost("Door/CreateDoor/{name}")]
        public async Task<IActionResult> CreateDoor(string name)
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

            // Construct the request URL with parameters
            var requestUrl = $"{baseUrl}entity?q=entity=NewEntity(Door),Name={Uri.EscapeDataString(name)},Guid";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Specify JSON response format

                try
                {
                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to create door.");
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
        [HttpPost("Door/AddAccessRule")]
        public async Task<IActionResult> AddAccessRule([FromBody] DoorAccessRuleRemovalRequest entity)
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

            // Construct the request URL with parameters
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(entity.Door)},AddAccessRule({Uri.EscapeDataString(entity.AccessRule)})";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Specify JSON response format

                try
                {
                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to add access rule.");
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
        [HttpPost("Door/AddAccessRuleWithSide")]
        public async Task<IActionResult> AddAccessRuleWithSide([FromBody] AccessRuleRequest request)
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

            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(request.Door)},AddAccessRule({Uri.EscapeDataString(request.AccessRule)},{Uri.EscapeDataString(request.Side)})";

            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Specify JSON response format

                try
                {
                    var response = await client.SendAsync(httpRequest);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(responseBody);
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, responseBody);
                    }
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpPost("Door/GetAccessRulesForSide")]
        public async Task<IActionResult> GetAccessRulesForSide([FromBody] DoorAccessRulesSideRequest request)
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

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(request.Door)},FetchAccessRulesForSide({Uri.EscapeDataString(request.AccessRuleSide)})";

            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Specify JSON response format

                try
                {
                    var response = await client.SendAsync(httpRequest);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(responseBody);
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, responseBody);
                    }
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpPost("Door/RemoveAccessRule")]
        public async Task<IActionResult> RemoveAccessRule([FromBody] DoorAccessRuleRemovalRequest request)
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

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(request.Door)},RemoveAccessRule({Uri.EscapeDataString(request.AccessRule)})";

            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Specify JSON response format

                try
                {
                    var response = await client.SendAsync(httpRequest);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(responseBody);
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, responseBody);
                    }
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        /* Access rule sides:
            In
            Out
            Both
        */
        [HttpPost("Door/RemoveAccessRuleBySide")]
        public async Task<IActionResult> RemoveAccessRuleBySide([FromBody] AccessRuleRequest request)
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

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(request.Door)},RemoveAccessRule({Uri.EscapeDataString(request.AccessRule)},{Uri.EscapeDataString(request.Side)})";

            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Specify JSON response format

                try
                {
                    var response = await client.SendAsync(httpRequest);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(responseBody);
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, responseBody);
                    }
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        /*Access point types
            CardReader
            Rex
            DoorSensor
            AreaDirection
            PullStation
            DoorLock
            Zone
            Floor
            Buzzer
            FloorInput
            FloorOutput
            EntrySensor
            PlateReader
            IntrusionAreaArming
            IntrusionAreaDirection
            LockSensor
            OperatorConfirmation
        */
        [HttpPost("Door/AddConnection")]
        public async Task<IActionResult> AddConnection([FromBody] DoorConnectionRequest request)
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

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(request.Door)},AddConnection({Uri.EscapeDataString(request.Device)},{Uri.EscapeDataString(request.AccessPointType)})";

            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Specify JSON response format

                try
                {
                    var response = await client.SendAsync(httpRequest);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(responseBody);
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, responseBody);
                    }
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpPost("Door/UpdateConnection")]
        public async Task<IActionResult> UpdateConnection([FromBody] UpdateConnectionRequest request)
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

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(request.Door)},UpdateConnection({Uri.EscapeDataString(request.AccessPoint)},{Uri.EscapeDataString(request.Device)})";

            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Specify JSON response format

                try
                {
                    var response = await client.SendAsync(httpRequest);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(responseBody);
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, responseBody);
                    }
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpPost("Door/RemoveConnection")]
        public async Task<IActionResult> RemoveConnection([FromBody] RemoveConnectionRequest request)
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

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(request.Door)},RemoveConnection({Uri.EscapeDataString(request.AccessPoint)})";

            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Specify JSON response format

                try
                {
                    var response = await client.SendAsync(httpRequest);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(responseBody);
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, responseBody);
                    }
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpPost("Door/RemoveConnectionByDevice")]
        public async Task<IActionResult> RemoveConnectionByDevice([FromBody] RemoveConnectionByDeviceRequest request)
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

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(request.Door)},RemoveConnectionByDevice({Uri.EscapeDataString(request.Device)})";

            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

                try
                {
                    var response = await client.SendAsync(httpRequest);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(responseBody);
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, responseBody);
                    }
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpPost("Door/AssignToArea")]
        public async Task<IActionResult> AssignToArea([FromQuery] string door, [FromQuery] string area)
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

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(door)},AddEntityToAreaCaptiveDoor({Uri.EscapeDataString(area)})";

            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response

                try
                {
                    var response = await client.SendAsync(httpRequest);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(responseBody);
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, responseBody);
                    }
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
