using App.Common.Extensions;
using App.Entity.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace VRFIDandGenetecIntegrationAPI.Controllers
{
    [ApiController]
    [SkipCustomerAuthorization]
    public class PassCreatorAPIController : Controller
    {

        private readonly IConfiguration _configuration;

        // Inject IConfiguration via constructor
        public PassCreatorAPIController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #region PassCreatorAPI GET

        [HttpGet("Passcreator/GetListOfTemplates")]
        public async Task<IActionResult> GetListOfTemplates()
        {
            try
            {
                using var client = new HttpClient();

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Get, "https://app.passcreator.com/api/pass-template");

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Use the Authorization header correctly
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve Templates information.");
                }

                // Handle invalid/missing character set
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                string responseBody = Encoding.UTF8.GetString(responseBytes);

                return Ok(responseBody);
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("Passcreator/GetPassTemplateFields/{identifier}")]
        public async Task<IActionResult> GetPassTemplateFields(string identifier)
        {
            try
            {
                using var client = new HttpClient();

                // Validate the identifier
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    return BadRequest("The 'identifier' parameter is required.");
                }

                // Construct the API URL with the identifier
                string baseUrl = "https://app.passcreator.com/api/pass-template/";
                string apiUrl = $"{baseUrl}{identifier}?zapierStyle=true";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve pass-template fields.");
                }

                // Handle the response
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                string responseBody = System.Text.Encoding.UTF8.GetString(responseBytes);

                return Ok(responseBody);
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("Passcreator/GetPassDetails/{passUid}")]
        public async Task<IActionResult> GetPassDetails(string passUid)
        {
            try
            {
                using var client = new HttpClient();

                // Validate pass UID
                if (string.IsNullOrWhiteSpace(passUid))
                {
                    return BadRequest("Pass UID is required.");
                }

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Construct the URL for the Pass Details API
                string apiUrl = $"https://app.passcreator.com/api/pass/{passUid}?includeFieldMapping=true";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Failed to retrieve pass details: {errorDetails}");
                }

                // Parse and return the response
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                string responseBody = System.Text.Encoding.UTF8.GetString(responseBytes);
                var passDetails = System.Text.Json.JsonSerializer.Deserialize<PassDetailsResponse>(
                    responseBody,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // Ensures case insensitivity during deserialization
                    });

                return Ok(passDetails);
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("Passcreator/GetAllPassesOfTemplate/{passTemplateUid}")]
        public async Task<IActionResult> GetAllPassesOfTemplate(
                                                                string passTemplateUid,
                                                                [FromQuery] int start = 0,
                                                                [FromQuery] int pageSize = 100,
                                                                [FromQuery] string lastIdOfPriorPage = null,
                                                                [FromQuery] string lastCreatedOnOfPriorPage = null,
                                                                [FromQuery] string createdSince = null,
                                                                [FromQuery] string modifiedSince = null
        )
        {
            try
            {
                using var client = new HttpClient();

                // Validate the pass template UID
                if (string.IsNullOrWhiteSpace(passTemplateUid))
                {
                    return BadRequest("Pass template UID is required.");
                }

                // Construct the query parameters
                var queryParameters = new List<string>
                {
                    $"start={start}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrWhiteSpace(lastIdOfPriorPage))
                    queryParameters.Add($"lastIdOfPriorPage={lastIdOfPriorPage}");

                if (!string.IsNullOrWhiteSpace(lastCreatedOnOfPriorPage))
                    queryParameters.Add($"lastCreatedOnOfPriorPage={lastCreatedOnOfPriorPage}");

                if (!string.IsNullOrWhiteSpace(createdSince))
                    queryParameters.Add($"createdSince={createdSince}");

                if (!string.IsNullOrWhiteSpace(modifiedSince))
                    queryParameters.Add($"modifiedSince={modifiedSince}");

                string queryString = string.Join("&", queryParameters);
                string apiUrl = $"https://app.passcreator.com/api/pass/list/{passTemplateUid}?{queryString}";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve passes.");
                }

                // Deserialize the response content
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                string responseBody = System.Text.Encoding.UTF8.GetString(responseBytes);
                var passes = JsonConvert.DeserializeObject<PassListResponse>(responseBody);

                return Ok(passes);
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("Passcreator/GetPassesOfMultipleTemplates/{passTemplateUids}")]
        public async Task<IActionResult> GetPassesOfMultipleTemplates(
            string passTemplateUids,
            [FromQuery] int start = 0,
            [FromQuery] int pageSize = 100,
            [FromQuery] string lastIdOfPriorPage = null,
            [FromQuery] string lastCreatedOnOfPriorPage = null,
            [FromQuery] string createdSince = null,
            [FromQuery] string modifiedSince = null)
        {
            try
            {
                using var client = new HttpClient();

                // Validate the list of pass template UIDs
                if (string.IsNullOrWhiteSpace(passTemplateUids))
                {
                    return BadRequest("At least one pass template UID is required.");
                }
                // Convert the comma-separated string to a list
                var passTemplateUidList = passTemplateUids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                          .Select(uid => uid.Trim())
                                                          .ToList();

                if (passTemplateUidList.Count == 0)
                {
                    return BadRequest("At least one valid pass template UID is required.");
                }
                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Prepare a list to store the responses
                var passListResponses = new List<PassListResponse>();

                foreach (var passTemplateUid in passTemplateUidList)
                {
                    // Construct the query parameters
                    var queryParameters = new List<string>
                    {
                        $"start={start}",
                        $"pageSize={pageSize}"
                    };

                    if (!string.IsNullOrWhiteSpace(lastIdOfPriorPage))
                        queryParameters.Add($"lastIdOfPriorPage={lastIdOfPriorPage}");

                    if (!string.IsNullOrWhiteSpace(lastCreatedOnOfPriorPage))
                        queryParameters.Add($"lastCreatedOnOfPriorPage={lastCreatedOnOfPriorPage}");

                    if (!string.IsNullOrWhiteSpace(createdSince))
                        queryParameters.Add($"createdSince={createdSince}");

                    if (!string.IsNullOrWhiteSpace(modifiedSince))
                        queryParameters.Add($"modifiedSince={modifiedSince}");

                    string queryString = string.Join("&", queryParameters);
                    string apiUrl = $"https://app.passcreator.com/api/pass/list/{passTemplateUid}?{queryString}";

                    // Create the API request
                    var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                    request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                    // Send the request
                    var response = await client.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, $"Failed to retrieve passes for template ID {passTemplateUid}.");
                    }

                    // Deserialize the response content
                    var responseBytes = await response.Content.ReadAsByteArrayAsync();
                    string responseBody = System.Text.Encoding.UTF8.GetString(responseBytes);
                    var passes = JsonConvert.DeserializeObject<PassListResponse>(responseBody);

                    if (passes != null)
                    {
                        passListResponses.Add(passes);
                    }
                }

                return Ok(passListResponses);
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("Passcreator/GetPassesOfMultipleTemplatesV2/{passTemplateUids}")]
        public async Task<IActionResult> GetPassesOfMultipleTemplatesV2(
            string passTemplateUids,
            [FromQuery] int start = 0,
            [FromQuery] int pageSize = 100,
            [FromQuery] string lastIdOfPriorPage = null,
            [FromQuery] string lastCreatedOnOfPriorPage = null,
            [FromQuery] string createdSince = null,
            [FromQuery] string modifiedSince = null)
        {
            try
            {
                using var client = new HttpClient();

                // Validate the list of pass template UIDs
                if (string.IsNullOrWhiteSpace(passTemplateUids))
                {
                    return BadRequest("At least one pass template UID is required.");
                }
                // Convert the comma-separated string to a list
                var passTemplateUidList = passTemplateUids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                          .Select(uid => uid.Trim())
                                                          .ToList();

                if (passTemplateUidList.Count == 0)
                {
                    return BadRequest("At least one valid pass template UID is required.");
                }
                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Prepare a list to store the responses
                var passListResponses = new List<Result>();

                foreach (var passTemplateUid in passTemplateUidList)
                {
                    // Construct the query parameters
                    var queryParameters = new List<string>
                    {
                        $"start={start}",
                        $"pageSize={pageSize}"
                    };

                    if (!string.IsNullOrWhiteSpace(lastIdOfPriorPage))
                        queryParameters.Add($"lastIdOfPriorPage={lastIdOfPriorPage}");

                    if (!string.IsNullOrWhiteSpace(lastCreatedOnOfPriorPage))
                        queryParameters.Add($"lastCreatedOnOfPriorPage={lastCreatedOnOfPriorPage}");

                    if (!string.IsNullOrWhiteSpace(createdSince))
                        queryParameters.Add($"createdSince={createdSince}");

                    if (!string.IsNullOrWhiteSpace(modifiedSince))
                        queryParameters.Add($"modifiedSince={modifiedSince}");

                    string queryString = string.Join("&", queryParameters);
                    string apiUrl = $"https://app.passcreator.com/api/pass/list/{passTemplateUid}?{queryString}";

                    // Create the API request
                    var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                    request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                    // Send the request
                    var response = await client.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, $"Failed to retrieve passes for template ID {passTemplateUid}.");
                    }

                    // Deserialize the response content
                    var responseBytes = await response.Content.ReadAsByteArrayAsync();
                    string responseBody = System.Text.Encoding.UTF8.GetString(responseBytes);
                    var passes = JsonConvert.DeserializeObject<PassListResponse>(responseBody);

                    if (passes != null)
                    {
                        passListResponses.AddRange(passes.Results);
                    }
                }

                return Ok(passListResponses);
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("Passcreator/GetPassUris/{passUid}")]
        public async Task<IActionResult> GetPassUris(string passUid)
        {
            try
            {
                using var client = new HttpClient();

                // Construct the API URL
                var apiUrl = $"https://app.passcreator.com/api/pass/geturis/{passUid}";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve pass URIs.");
                }

                // Parse response
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                string responseBody = Encoding.UTF8.GetString(responseBytes);

                // Deserialize the JSON response
                var passUris = System.Text.Json.JsonSerializer.Deserialize<PassUrisResponse>(responseBody);

                if (passUris == null)
                {
                    return StatusCode(500, "Failed to parse the response.");
                }

                // Decode the Android URI
                passUris.AndroidUri = Uri.UnescapeDataString(passUris.AndroidUri);
                passUris.iPhoneUri = Uri.UnescapeDataString(passUris.iPhoneUri);

                // Return the corrected response
                return Ok(passUris);
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("Passcreator/SearchPass/{templateUid}/{searchString}")]
        public async Task<IActionResult> SearchPass(string templateUid, string searchString)
        {
            try
            {
                using var client = new HttpClient();

                // Construct the API URL
                var apiUrl = $"https://app.passcreator.com/api/pass/search/{templateUid}/{searchString}";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to search for passes.");
                }

                // Parse the response
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                string responseBody = Encoding.UTF8.GetString(responseBytes);

                
                // Return the search results
                return Ok(responseBody);
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("Passcreator/GetPassStatistics/{templateUid}")]
        public async Task<IActionResult> GetPassStatistics(string templateUid, [FromQuery] string timeFrame, [FromQuery] string day)
        {
            try
            {
                using var client = new HttpClient();

                // Construct the API URL
                var apiUrl = $"https://app.passcreator.com/api/pass/statistics/{templateUid}?timeFrame={timeFrame}&day={day}";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve pass statistics.");
                }

                // Parse the response
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                string responseBody = Encoding.UTF8.GetString(responseBytes);

                // Deserialize the JSON response
                var statistics = System.Text.Json.JsonSerializer.Deserialize<PassStatisticsResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (statistics == null)
                {
                    return NotFound("No statistics data found.");
                }

                // Return the statistics data
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("Passcreator/GetActiveHistory/{templateUid}")]
        public async Task<IActionResult> GetActiveHistory(string templateUid, [FromQuery] string startingDay = null)
        {
            try
            {
                using var client = new HttpClient();

                // Construct the API URL
                var apiUrl = $"https://app.passcreator.com/api/pass/statistics/{templateUid}/activehistory";
                if (!string.IsNullOrWhiteSpace(startingDay))
                {
                    apiUrl += $"/{startingDay}";
                }

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve pass active history.");
                }

                // Parse the response
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                string responseBody = Encoding.UTF8.GetString(responseBytes);

                // Deserialize the JSON response
                var historyData = System.Text.Json.JsonSerializer.Deserialize<List<ActiveHistoryEntry>>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (historyData == null || !historyData.Any())
                {
                    return NotFound("No active history data found.");
                }

                // Return the history data
                return Ok(historyData);
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region PassCreatorAPI POST
        [HttpPost("Passcreator/CreatePass/{passTemplateUid}")]
        public async Task<IActionResult> CreatePassAsync(string passTemplateUid, [FromBody] Dictionary<string, string> additionalProperties)
        {
            try
            {
                using var client = new HttpClient();

                // Validate the passTemplateUid
                if (string.IsNullOrWhiteSpace(passTemplateUid))
                {
                    return BadRequest("The 'passTemplateUid' parameter is required.");
                }

                // Step 1: Fetch the template fields to get the keys
                var passTemplateFieldsResponse = await GetAllPassTemplateFields(passTemplateUid);
                if (passTemplateFieldsResponse is not OkObjectResult okResponse)
                {
                    return passTemplateFieldsResponse;
                }

                var fieldKeys = okResponse.Value as dynamic;
                string firstNameKey = fieldKeys.FirstNameKey;
                string lastNameKey = fieldKeys.LastNameKey;
                string emailKey = fieldKeys.EmailKey;

                // Step 2: Prepare the data to send in the request
                var requestBody = new Dictionary<string, string>
                {
                    { firstNameKey, additionalProperties.ContainsKey("FirstName") ? additionalProperties["FirstName"] : "" },
                    { lastNameKey, additionalProperties.ContainsKey("LastName") ? additionalProperties["LastName"] : "" },
                    { emailKey, additionalProperties.ContainsKey("Email") ? additionalProperties["Email"] : "" }
                };

                // Construct the API URL for creating the pass
                string apiUrl = $"https://app.passcreator.com/api/pass?passtemplate={passTemplateUid}&zapierStyle=true";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Serialize the request body to JSON
                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                request.Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                // Send the request
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to create the pass.");
                }

                // Handle the response
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                string responseBody = System.Text.Encoding.UTF8.GetString(responseBytes);

                return Ok(responseBody);
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("Passcreator/EnableDownload/{passUid}")]
        public async Task<IActionResult> EnableDownloadAsync(string passUid, [FromBody] EnableDownloadRequest enableDownloadRequest)
        {
            try
            {
                using var client = new HttpClient();

                // Validate the pass UID
                if (string.IsNullOrWhiteSpace(passUid))
                {
                    return BadRequest("Pass UID is required.");
                }

                // Construct the URL for the Enable Download API
                string apiUrl = $"https://app.passcreator.com/api/pass/{passUid}/enabledownload";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Prepare the request body
                var requestBody = new
                {
                    // Use provided EnableDownloadUntil or default to 10 minutes from now
                    enableDownloadUntil = enableDownloadRequest?.EnableDownloadUntil
                                          ?? DateTime.UtcNow.AddMinutes(10).ToString("yyyy-MM-dd HH:mm")
                };

                // Set the request content
                request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                // Send the request
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to enable download for the pass.");
                }

                // Return Status 204 indicating success
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("Passcreator/SendPushNotification/{passUid}")]
        public async Task<IActionResult> SendPushNotification(string passUid, [FromBody] PushNotificationRequest requestBody)
        {
            try
            {
                using var client = new HttpClient();

                // Validate inputs
                if (string.IsNullOrWhiteSpace(passUid))
                {
                    return BadRequest("Pass UID is required.");
                }

                if (requestBody == null || string.IsNullOrWhiteSpace(requestBody.PushNotificationText))
                {
                    return BadRequest("Push notification text is required.");
                }

                // Construct the API URL
                string apiUrl = $"https://app.passcreator.com/api/pass/{passUid}/sendpushnotification";

                // Create the HTTP request
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
                };

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Successfully sent the push notification
                    return NoContent(); // HTTP 204
                }

                // Handle API errors
                return StatusCode((int)response.StatusCode, "Failed to send push notification.");
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("Passcreator/SendPushNotifications")]
        public async Task<IActionResult> SendPushNotifications([FromBody] MultiPushNotificationRequest requestBody)
        {
            try
            {
                using var client = new HttpClient();

                // Validate the request body
                if (requestBody == null || requestBody.ListOfPasses == null || !requestBody.ListOfPasses.Any())
                {
                    return BadRequest("List of passes is required.");
                }

                if (string.IsNullOrWhiteSpace(requestBody.PushNotificationText))
                {
                    return BadRequest("Push notification text is required.");
                }

                if (requestBody.ListOfPasses.Count > 500)
                {
                    return BadRequest("You cannot specify more than 500 passes in one request.");
                }

                // Construct the API URL
                string apiUrl = "https://app.passcreator.com/api/pass/sendpushnotifications";

                // Create the HTTP request
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
                };

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Successfully scheduled push notifications
                    return Accepted(); // HTTP 202
                }

                // Handle API errors
                return StatusCode((int)response.StatusCode, "Failed to send push notifications.");
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("Passcreator/MovePassToTemplate/{identifier}/{targetTemplateIdentifier}")]
        public async Task<IActionResult> MovePassToTemplate(string identifier, string targetTemplateIdentifier)
        {
            try
            {
                using var client = new HttpClient();

                // Construct the API URL
                var apiUrl = $"https://app.passcreator.com/api/v2/pass/{identifier}/movetotemplate/{targetTemplateIdentifier}";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    // Read the error response
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Failed to move pass to template. Error: {errorResponse}");
                }

                // Parse the response
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                string responseBody = Encoding.UTF8.GetString(responseBytes);

                // Deserialize the JSON response
                var movePassResponse = System.Text.Json.JsonSerializer.Deserialize<MovePassResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (movePassResponse == null || !movePassResponse.Success)
                {
                    return BadRequest("Failed to move the pass to the target template.");
                }

                // Return success response
                return Ok(movePassResponse);
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("Passcreator/SendPassViaEmail/{identifier}/{recipientMail}")]
        public async Task<IActionResult> SendPassViaEmail(string identifier, string recipientMail)
        {
            try
            {
                using var client = new HttpClient();

                // Construct the API URL
                var apiUrl = $"https://app.passcreator.com/api/pass/deliver/{identifier}/email/{recipientMail}";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);

                // Handle different status codes
                if (response.IsSuccessStatusCode)
                {
                    return Ok("Email has been sent successfully.");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return BadRequest($"Failed to send the email. Error: {errorResponse}");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound("No pass found with the provided identifier.");
                }

                // Other unexpected status codes
                return StatusCode((int)response.StatusCode, "An unexpected error occurred.");
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region PassCreatorAPI PUT
        [HttpPut("Passcreator/UpdatePassAsync/{passId}")]
        public async Task<IActionResult> UpdatePassAsync(string passId, [FromBody] UpdatePassRequest updateRequest)
        {
            try
            {
                using var client = new HttpClient();

                // Validate input parameters
                if (string.IsNullOrWhiteSpace(passId) || updateRequest == null)
                {
                    return BadRequest("Pass ID and update request data are required.");
                }

                // Get pass details to retrieve the pass template GUID
                var passDetailsResponse = await GetPassDetails(passId);
                if (passDetailsResponse is not OkObjectResult passDetailsResult)
                {
                    return BadRequest("Failed to retrieve pass details.");
                }

                var passDetails = passDetailsResult.Value as PassDetailsResponse;
                if (passDetails == null || string.IsNullOrWhiteSpace(passDetails.PassTemplateGuid))
                {
                    return BadRequest("Pass template GUID is required.");
                }

                string passTemplateGuid = passDetails.PassTemplateGuid;

                // Get the pass template fields
                var fieldsResponse = await GetPassTemplateFieldsMethod(passTemplateGuid);
                if (fieldsResponse is not OkObjectResult fieldsResult)
                {
                    return BadRequest("Failed to retrieve pass template fields.");
                }

                var fields = fieldsResult.Value as dynamic; // Use dynamic to access fields
                if (fields == null)
                {
                    return BadRequest("Invalid response from pass template fields.");
                }

                string firstNameKey = fields.FirstNameKey;
                string lastNameKey = fields.LastNameKey;
                string emailKey = fields.EmailKey;

                // Construct the update request payload
                var updatePayload = new Dictionary<string, string>
                {
                    { firstNameKey, updateRequest.FirstName },
                    { lastNameKey, updateRequest.LastName },
                    { emailKey, updateRequest.Email }
                };

                // Send the update request to PassCreator API
                string apiUrl = $"https://app.passcreator.com/api/pass/{passId}?zapierStyle=true";
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(updatePayload), System.Text.Encoding.UTF8, "application/json")
                };

                // Add the API key to the headers
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to update pass.");
                }

                return Ok("Pass updated successfully.");
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPut("Passcreator/MarkPassVoided/{passUid}")]
        public async Task<IActionResult> MarkPassVoided(string passUid, [FromBody] UpdatePassVoidRequest requestBody)
        {
            try
            {
                using var client = new HttpClient();

                // Validate the pass UID
                if (string.IsNullOrWhiteSpace(passUid))
                {
                    return BadRequest("Pass UID is required.");
                }

                // Construct the API URL
                string apiUrl = $"https://app.passcreator.com/api/pass/{passUid}";

                // Create the HTTP request
                var request = new HttpRequestMessage(HttpMethod.Put, apiUrl)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
                };

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Successfully marked the pass as voided
                    return NoContent(); // HTTP 204
                }

                // Handle API errors
                return StatusCode((int)response.StatusCode, "Failed to update the pass.");
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region PassCreatorAPI DELETE
        [HttpDelete("Passcreator/DeletePass/{passUid}")]
        public async Task<IActionResult> DeletePass(string passUid)
        {
            try
            {
                using var client = new HttpClient();

                // Validate the pass UID
                if (string.IsNullOrWhiteSpace(passUid))
                {
                    return BadRequest("Pass UID is required.");
                }

                // Construct the URL for the Pass Delete API
                string apiUrl = $"https://app.passcreator.com/api/pass/{passUid}";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Delete, apiUrl);

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    // Successfully deleted the pass
                    return Ok("Pass deleted successfully.");
                }

                // If deletion fails, return an error with status code and message
                return StatusCode((int)response.StatusCode, "Failed to delete the pass.");
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region Methods
        [HttpGet("Passcreator/GetPassTemplateFieldsMethod/{identifier}")]
        public async Task<IActionResult> GetPassTemplateFieldsMethod(string identifier)
        {
            try
            {
                using var client = new HttpClient();

                // Validate the identifier
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    return BadRequest("Template identifier is required.");
                }

                // Construct the URL for the Pass Template Fields API
                string apiUrl = $"https://app.passcreator.com/api/pass-template/{identifier}?zapierStyle=true";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve pass template fields.");
                }

                // Deserialize the response content to get fields
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                string responseBody = System.Text.Encoding.UTF8.GetString(responseBytes);
                var fields = JsonConvert.DeserializeObject<List<PassTemplateField>>(responseBody);

                // Find the keys for First name, Last name, and Email address
                var firstNameKey = fields.FirstOrDefault(f => f.label == "First name")?.key;
                var lastNameKey = fields.FirstOrDefault(f => f.label == "Last name")?.key;
                var emailKey = fields.FirstOrDefault(f => f.label == "Email address")?.key;

                // If any key is not found, return an error
                if (firstNameKey == null || lastNameKey == null || emailKey == null)
                {
                    return BadRequest("One or more required fields are missing in the template.");
                }

                // Return the keys as part of the response
                return Ok(new
                {
                    FirstNameKey = firstNameKey,
                    LastNameKey = lastNameKey,
                    EmailKey = emailKey
                });
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("Passcreator/GetAllPassTemplateFields/{identifier}")]
        public async Task<IActionResult> GetAllPassTemplateFields(string identifier)
        {
            try
            {
                using var client = new HttpClient();

                // Validate the identifier
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    return BadRequest("Template identifier is required.");
                }

                // Construct the URL for the Pass Template Fields API
                string apiUrl = $"https://app.passcreator.com/api/pass-template/{identifier}?zapierStyle=true";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve pass template fields.");
                }

                // Deserialize the response content to get fields
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                string responseBody = System.Text.Encoding.UTF8.GetString(responseBytes);
                var fields = JsonConvert.DeserializeObject<List<PassTemplateField>>(responseBody);

                // Map field keys to a list
                var fieldKeys = fields.Select(f => f.key).ToList();

                // Return all field keys
                return Ok(new { FieldKeys = fieldKeys });
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("Passcreator/GetAllPassTemplateFieldsV2/{identifier}")]
        public async Task<IActionResult> GetAllPassTemplateFieldsV2(string identifier)
        {
            try
            {
                using var client = new HttpClient();

                // Validate the identifier
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    return BadRequest("Template identifier is required.");
                }

                // Construct the URL for the Pass Template Fields API
                string apiUrl = $"https://app.passcreator.com/api/pass-template/{identifier}?zapierStyle=true";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to retrieve pass template fields.");
                }

                // Deserialize the response content to get fields
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                string responseBody = System.Text.Encoding.UTF8.GetString(responseBytes);
                var fields = JsonConvert.DeserializeObject<List<PassTemplateField>>(responseBody);

                // Map field keys to a list
                //var fieldKeys = fields.Select(f => f.key).ToList();

                // Return all field keys
                return Ok(fields);
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("Passcreator/CreatePassV2/{passTemplateUid}")]
        public async Task<IActionResult> CreatePassAsyncV2(string passTemplateUid, [FromBody] Dictionary<string, string> additionalProperties)
        {
            try
            {
                using var client = new HttpClient();

                // Validate the passTemplateUid
                if (string.IsNullOrWhiteSpace(passTemplateUid))
                {
                    return BadRequest("The 'passTemplateUid' parameter is required.");
                }

                // Step 1: Fetch all field keys using the GetAllPassTemplateFields method
                var passTemplateFieldsResponse = await GetAllPassTemplateFields(passTemplateUid);
                if (passTemplateFieldsResponse is not OkObjectResult okResponse)
                {
                    return passTemplateFieldsResponse;
                }
                
                // Extract field keys from the response
                var fieldKeys = ((dynamic)okResponse.Value).FieldKeys as List<string>;
                if (fieldKeys == null || !fieldKeys.Any())
                {
                    return BadRequest("No field keys found for the specified pass template.");
                }

                // Step 2: Prepare the request body by mapping additionalProperties with field keys
                var requestBody = new Dictionary<string, string>();
                foreach (var fieldKey in fieldKeys)
                {
                    // Map the field key with the provided value or leave it blank
                    requestBody[fieldKey] = additionalProperties.ContainsKey(fieldKey) ? additionalProperties[fieldKey] : "";
                }

                // Construct the API URL for creating the pass
                string apiUrl = $"https://app.passcreator.com/api/pass?passtemplate={passTemplateUid}&zapierStyle=true";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Add the Authorization header
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Serialize the request body to JSON
                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);
                request.Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                // Step 3: Send the request to create the pass
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to create the pass.");
                }

                // Handle the response
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                string responseBody = System.Text.Encoding.UTF8.GetString(responseBytes);

                return Ok(responseBody);
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return StatusCode(500, ex.Message);
            }
        }


        //Method:
        [HttpGet("Passcreator/GetPassDetailsForGenetec/{passUid}")]
        public async Task<PassDetailsResponse> GetPassDetailsForGenetec(string passUid)
        {
            try
            {
                using var client = new HttpClient();

                // Validate pass UID
                if (string.IsNullOrWhiteSpace(passUid))
                {
                    return null;
                }

                // Retrieve the API key from appsettings.json
                string apiKey = _configuration["PassCreator:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    throw new Exception("API Key not found in configuration.");
                }

                // Construct the URL for the Pass Details API
                string apiUrl = $"https://app.passcreator.com/api/pass/{passUid}?includeFieldMapping=true";

                // Create the API request
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                request.Headers.TryAddWithoutValidation("Authorization", apiKey);

                // Send the request
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    return null;
                }

                // Parse and return the response
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                string responseBody = System.Text.Encoding.UTF8.GetString(responseBytes);
                var passDetails = System.Text.Json.JsonSerializer.Deserialize<PassDetailsResponse>(
                    responseBody,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // Ensures case insensitivity during deserialization
                    });

                return passDetails;
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine($"Exception: {ex}");
                return null;
            }
        }
        #endregion
    }
}
