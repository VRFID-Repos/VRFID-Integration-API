using App.Bal.Repositories;
using App.Bal.Services;
using App.Common;
using App.Common.Extensions;
using App.Entity.Models;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;

namespace VRFIDandGenetecIntegrationAPI.Controllers
{
    [ApiController]
    public class CredentialController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGenetecServices _genetecServices;
        private readonly CardHolderController _cardHolderController;
        private readonly GroupController _cardHolderGroupController;
        private readonly PassCreatorAPIController _passCreatorAPIController;

        // Inject the configuration
        public CredentialController(IGenetecServices genetecServices,IConfiguration configuration,IServiceProvider serviceProvider, CardHolderController cardHolderController, GroupController cardHolderGroupController, PassCreatorAPIController passCreatorAPIController)
        {
            _configuration = configuration;
            _cardHolderController = cardHolderController;
            _cardHolderGroupController = cardHolderGroupController;
            _passCreatorAPIController = passCreatorAPIController;
            _serviceProvider = serviceProvider;
            _genetecServices = genetecServices;
        }
        #region CredentialAPI
        //GET:
        [HttpGet("Credential/FindCredentialByLicensePlate")]
        public async Task<IActionResult> FindCredentialByLicensePlate([FromQuery] LicensePlateCredentialRequestModel model)
        {
            if (string.IsNullOrEmpty(model.LicensePlate))
            {
                return BadRequest("License plate number is required.");
            }

            // Retrieve API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the license plate parameter
            var encodedLicensePlate = HttpUtility.UrlEncode($"UniqueIds@LicensePlateCredentialFormat({model.LicensePlate})");

            // Construct the request URL
            var requestUrl = $"{baseUrl}report/CredentialConfiguration?q={encodedLicensePlate}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Specify JSON response format

                try
                {
                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to retrieve credential information.");
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
        [HttpGet("Credential/GetCredential/{credentialId}")]
        public async Task<IActionResult> GetCredential(string credentialId)
        {
            if (string.IsNullOrEmpty(credentialId))
            {
                return BadRequest("Credential ID is required.");
            }

            // Retrieve API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity/{credentialId}";

            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Specify JSON response format

                try
                {
                    var response = await client.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to retrieve credential information.");
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
       
        //POST:
        [HttpPost("Credential/CreateLicensePlateCredential")]
        public async Task<IActionResult> CreateLicensePlateCredential([FromBody] EntityCredentials model)
        {
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.LicensePlate))
            {
                return BadRequest("Both credential name and license plate number are required.");
            }

            // Retrieve API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the parameters
            var encodedName = HttpUtility.UrlEncode($"Name={model.Name}");
            var encodedFormat = HttpUtility.UrlEncode($"Format=LicensePlateCredentialFormat({model.LicensePlate})");
            var queryParams = $"entity=NewEntity(Credential),{encodedName},{encodedFormat},Guid";

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

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
                        return StatusCode((int)response.StatusCode, "Failed to create credential with license plate.");
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
        [HttpPost("Credential/CreateCredentialWithPin")]
        public async Task<IActionResult> CreateCredentialWithPin([FromBody] CreateCredentialWithPinRequest request)
        {
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity=NewEntity(Credential),Name={Uri.EscapeDataString(request.Name)},Format=KeypadCredentialFormat({Uri.EscapeDataString(request.CredentialCode)}),Guid";

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
        [HttpPost("Credential/CreateCredentialWithCustomFormat")]
        public async Task<IActionResult> CreateCredentialWithCustomFormat([FromBody] CreateCredentialWithCustomFormatRequest request)
        {
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Construct key-value pairs dynamically from the dictionary
            var keyValuePairs = request.KeyValues
                .Select(kvp => $"{{\"{Uri.EscapeDataString(kvp.Key)}\":\"{Uri.EscapeDataString(kvp.Value)}\"}}")
                .ToList();

            var keyValuePairsString = string.Join(",", keyValuePairs);

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity=NewEntity(Credential)," +
                             $"Name={Uri.EscapeDataString(request.Name)}," +
                             $"Format=CustomCredentialFormat({Uri.EscapeDataString(request.FormatId)},{keyValuePairsString}),Guid";

            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); // Expect JSON response

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
        [HttpPost("Credential/CreateCredentialRequest")]
        public async Task<IActionResult> CreateCredentialRequest([FromBody] CreateCredentialRequest request)
        {
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity=NewEntity(Credential)," +
                             $"Name={Uri.EscapeDataString(request.Name)}," +
                             $"Format=CardRequestCredentialFormat(),Guid";

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
        [HttpPost("Credential/GetCredentialRequest")]
        public async Task<IActionResult> GetCredentialRequest([FromBody] GetCredentialRequest request)
        {
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(request.Credential)},GetCredentialRequest()";

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
        [HttpPost("Credential/CancelCredentialRequest")]
        public async Task<IActionResult> CancelCredentialRequest([FromBody] GetCredentialRequest request)
        {
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(request.Credential)},CancelCredentialRequest()";

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
        [HttpPost("Credential/ActivateCredential")]
        public async Task<IActionResult> ActivateCredential([FromBody] ActivateCredentialRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Credential))
            {
                return BadRequest("Credential GUID is required.");
            }

            // Retrieve API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the credential parameter
            var encodedCredential = HttpUtility.UrlEncode($"entity={model.Credential}");
            var queryParams = $"{encodedCredential},Status.Activate()";

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

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
                        return StatusCode((int)response.StatusCode, "Failed to activate the credential.");
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
        [HttpPost("Credential/DeactivateCredential")]
        public async Task<IActionResult> DeactivateCredential([FromBody] ActivateCredentialRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Credential))
            {
                return BadRequest("Credential GUID is required.");
            }

            // Retrieve API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the credential parameter
            var encodedCredential = HttpUtility.UrlEncode($"entity={model.Credential}");
            var queryParams = $"{encodedCredential},Status.Deactivate()";

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

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
                        return StatusCode((int)response.StatusCode, "Failed to deactivate the credential.");
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
        [HttpPost("Credential/DeactivateCredentialAtSpecificDate")]
        public async Task<IActionResult> DeactivateCredentialAtSpecificDate([FromBody] DeactivateCredentialRequest request)
        {
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Format the expiration date to be compatible with URL
            var expirationDateFormatted = Uri.EscapeDataString(request.ExpirationDate.ToString("yyyy-MM-ddTHH:mm:ss"));

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(request.Credential)},Status.Deactivate({expirationDateFormatted})";

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
        [HttpPost("Credential/ActivateCredentialAtSpecificDate")]
        public async Task<IActionResult> ActivateCredentialAtSpecificDate([FromBody] DeactivateCredentialRequest request)
        {
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Format the expiration date to be compatible with URL
            var expirationDateFormatted = Uri.EscapeDataString(request.ExpirationDate.ToString("yyyy-MM-ddTHH:mm:ss"));

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(request.Credential)},Status.Activate({expirationDateFormatted})";

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
        [HttpPost("Credential/AddActivationAndDeactivationDateOnCredential")]
        public async Task<IActionResult> AddActivationAndDeactivationDateOnCredential([FromBody] ActivateCredentialRequest request)
        {
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Format activation and expiration dates
            var activationDateFormatted = Uri.EscapeDataString(request.ActivationDate.ToString("yyyy-MM-ddTHH:mm:ss"));
            var expirationDateFormatted = Uri.EscapeDataString(request.ExpirationDate.ToString("yyyy-MM-ddTHH:mm:ss"));

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity={Uri.EscapeDataString(request.Credential)}," +
                             $"Status.Activate({activationDateFormatted}, {expirationDateFormatted})";

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

        [HttpPost("Credential/AssignCredentialToCardholder")]
        public async Task<IActionResult> AssignCredentialToCardholder([FromBody] AssignCredentialRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Cardholder) || string.IsNullOrEmpty(model.Credential))
            {
                return BadRequest("Cardholder and Credential GUIDs are required.");
            }

            // Retrieve API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the parameters
            var encodedParams = HttpUtility.UrlEncode($"entity={model.Cardholder},Credentials@{model.Credential}");

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={encodedParams}";

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
                        return StatusCode((int)response.StatusCode, "Failed to assign credential to the cardholder.");
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
        [HttpPost("Credential/UnassignCredentialFromCardholder")]
        public async Task<IActionResult> UnassignCredentialFromCardholder([FromBody] AssignCredentialRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Cardholder) || string.IsNullOrEmpty(model.Credential))
            {
                return BadRequest("Cardholder and Credential GUIDs are required.");
            }

            // Retrieve API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the parameters
            var encodedParams = HttpUtility.UrlEncode($"entity={model.Cardholder},Credentials-{model.Credential}");

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={encodedParams}";

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
                        return StatusCode((int)response.StatusCode, "Failed to unassign credential from the cardholder.");
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
        [HttpPost("Credential/CreateDigitalPassCredential")]
        public async Task<IActionResult> CreateDigitalPassCredential([FromBody] CreateDigitalPassCredentialRequest request)
        {
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Remove delimiters and validate identifier
            var identifier = request.Identifier.Replace("-", "").ToUpper();
            if (identifier.Length != 32)
            {
                return BadRequest("Invalid identifier. It must be exactly 32 hexadecimal characters after removing delimiters.");
            }

            // Extract fields
            var field1 = identifier.Substring(0, 8);
            var field2 = identifier.Substring(8, 4);
            var field3 = identifier.Substring(12, 4);
            var field4 = identifier.Substring(16, 4);
            var field5 = identifier.Substring(20, 12);

            // Construct the fields string as per documentation
            var fieldsString = $"{{Field1, {field1}}},{{Field2, {field2}}},{{Field3, {field3}}},{{Field4, {field4}}},{{Field5, {field5}}}";

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity=NewEntity(Credential)," +
                             $"Name={Uri.EscapeDataString(request.Name)}," +
                             $"Format=CustomCredentialFormat({Uri.EscapeDataString(request.FormatId)},{{{fieldsString}}}),Guid";


            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json"));

                try
                {
                    var response = await client.SendAsync(httpRequest);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var CredResponse = JsonConvert.DeserializeObject<ResponseModel>(responseBody);

                        return Ok(CredResponse); // Return the parsed JSON response directly
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
        [HttpPost("Credential/CreateWiegandCredential")]
        public async Task<IActionResult> CreateWiegandCredential([FromBody] CreateWiegandCredentialRequest request)
        {
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Validate raw data length (example validation, adjust as per requirements)
            if (string.IsNullOrWhiteSpace(request.RawData))
            {
                return BadRequest("RawData cannot be null or empty.");
            }

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity=NewEntity(Credential)," +
                             $"Name={Uri.EscapeDataString(request.Name)}," +
                             $"Format=<UndecodedWiegand><BitLength>{request.BitLength}</BitLength><FormatType>{Uri.EscapeDataString(request.CustomFormatId)}</FormatType><Raw>{Uri.EscapeDataString(request.RawData)}</Raw></UndecodedWiegand>,Guid";

            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json"));

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

        //----------------
        [HttpPost("Credential/CreateLicensePlateCredentialForNewBook")]
        public async Task<IActionResult> CreateLicensePlateCredentialForNewBook([FromBody] EntityCredentials model)
        {
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.LicensePlate))
            {
                return BadRequest("Both credential name and license plate number are required.");
            }

            // Retrieve API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the parameters
            var encodedName = HttpUtility.UrlEncode($"Name={model.Name}");
            var encodedFormat = HttpUtility.UrlEncode($"Format=LicensePlateCredentialFormat({model.LicensePlate})");
            var queryParams = $"entity=NewEntity(Credential),{encodedName},{encodedFormat},Guid";

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

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
                        return StatusCode((int)response.StatusCode, "Failed to create credential with license plate.");
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var LicenseCredResponse = JsonConvert.DeserializeObject<ResponseModel>(responseBody);

                    return Ok(LicenseCredResponse); // Return the parsed JSON response directly
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpPost("Credential/CreateCredentialWithPinForNewBook")]
        public async Task<IActionResult> CreateCredentialWithPinForNewBook([FromBody] CreateCredentialWithPinRequest request)
        {
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity=NewEntity(Credential),Name={Uri.EscapeDataString(request.Name)},Format=KeypadCredentialFormat({Uri.EscapeDataString(request.CredentialCode)}),Guid";

            using (var client = new HttpClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
                httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json")); // Expect JSON response
                try
                {
                    var response = await client.SendAsync(httpRequest);

                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to create credential with license plate.");
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var PinCredResponse = JsonConvert.DeserializeObject<ResponseModel>(responseBody);

                    return Ok(PinCredResponse); // Return the parsed JSON response directly
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }
        [HttpPost("Credential/CreateLicensePlateCredential2")]
        public async Task<IActionResult> CreateLicensePlateCredential2([FromBody] EntityCredentials2 model)
        {
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.LicensePlate))
            {
                return BadRequest("Both credential name and license plate number are required.");
            }
            // Parse input dates as UTC
            DateTime activationDate = DateTime.SpecifyKind(
                DateTime.ParseExact(model.ActivationDateTime, "ddMMyy HHmm", CultureInfo.InvariantCulture),
                DateTimeKind.Utc
            );
            DateTime deactivationDate = DateTime.SpecifyKind(
                DateTime.ParseExact(model.DeactivationDateTime, "ddMMyy HHmm", CultureInfo.InvariantCulture),
                DateTimeKind.Utc
            );

            // Use Windows-compatible time zone ID
            string windowsTimeZoneId = "AUS Eastern Standard Time";
            TimeZoneInfo localZone;

            try
            {
                localZone = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                return StatusCode(500, $"Time zone ID '{windowsTimeZoneId}' not found on the server.");
            }
            catch (InvalidTimeZoneException)
            {
                return StatusCode(500, $"Time zone ID '{windowsTimeZoneId}' is invalid.");
            }

            // Convert UTC to local time
            DateTime activationLocal = TimeZoneInfo.ConvertTimeFromUtc(activationDate, localZone);
            DateTime deactivationLocal = TimeZoneInfo.ConvertTimeFromUtc(deactivationDate, localZone);

            var cardholderGuid = String.Empty;
            var licenseCredGuid = String.Empty;
            var PinCredGuid = String.Empty;
            var cardholderGroupGuid = "2cb29e64-f632-4795-bdcf-387051776812";
            //Create CardHolder
            var cardholder = new EntityCardholder
            {
                Name = model.Name,
                FirstName = model.Name,
                LastName = "",
                Email = "",
                MobilePhone = ""
            };
            // Call CreateCardholderFromNewBook and get the result
            var createCardholderResult = await _cardHolderController.CreateCardholderFromNewBook(cardholder);

            // Ensure the result is an OkObjectResult and contains the ResponseModel
            if (createCardholderResult is OkObjectResult okResult && okResult.Value is ResponseModel cardholderResponse)
            {
                cardholderGuid = cardholderResponse?.Rsp?.Result?.Guid;
            }
            else
            {
                return StatusCode(500, $"Request error: Unable to add Cardholder.");
            }
            //Add Activation and Deactivation Date
            if (deactivationDate > DateTime.MinValue && activationDate > DateTime.MinValue)
            {
                var AddCardholderActivationWithExpirationRequestModel = new CardholderActivationWithExpirationRequestModel
                {
                    Cardholder = cardholderGuid,
                    ActivationDate = activationDate,
                    ExpirationDate = deactivationDate
                };
                var SetActivationAndExpirationDatesForNewBookResult = 
                    await _cardHolderController.SetActivationAndExpirationDatesForNewBook(AddCardholderActivationWithExpirationRequestModel);
            }
            
            //Assign Group to CardHolder
            if (!String.IsNullOrEmpty(cardholderGroupGuid) && !String.IsNullOrEmpty(cardholderGuid))
            {
                var AssignCardToGroup = new CardholderGroupRequestModel
                {
                   Cardholder = cardholderGuid,
                   Group = cardholderGroupGuid
                };
                var AssignCardToGroupResult = await _cardHolderGroupController.AddCardholderToGroupForNewbook(AssignCardToGroup);
            }
            //Create License Credential
            var LicenseCredential = new EntityCredentials
            {
                Name = model.Name,
                LicensePlate = model.LicensePlate
            };
            var CreateLicensePlateCredentialResult = await CreateLicensePlateCredentialForNewBook(LicenseCredential);
            // Ensure the result is an OkObjectResult and contains the ResponseModel
            if (CreateLicensePlateCredentialResult is OkObjectResult okResult2 && okResult2.Value is ResponseModel LicenseCredResponse)
            {
                licenseCredGuid = LicenseCredResponse?.Rsp?.Result?.Guid;
            }
            else
            {
                return StatusCode(500, $"Request error: Unable to create Credential with license plate.");
            }

            //Create Pin Credential
            var PinCredential = new CreateCredentialWithPinRequest
            {
                Name = model.Name,
                CredentialCode = model.Pin
            };
            var CreatePinCredentialResult= await CreateCredentialWithPinForNewBook(PinCredential);
            if (CreatePinCredentialResult is OkObjectResult okResult3 && okResult3.Value is ResponseModel PinCredResponse)
            {
                PinCredGuid = PinCredResponse?.Rsp?.Result?.Guid;
            }
            else
            {
                return StatusCode(500, $"Request error: Unable to add Credential with pin.");
            }
            //Assign License Credential to CardHolder
            if (!String.IsNullOrEmpty(licenseCredGuid))
            {
                var AssignLicenseCred = new AssignCredentialRequestModel
                {
                    Cardholder = cardholderGuid,
                    Credential = licenseCredGuid
                };
                var AssignLicenseCredResult = await AssignCredentialToCardholder(AssignLicenseCred);
            }
            //Assign Pin Credential to CardHolder
            if (!String.IsNullOrEmpty(PinCredGuid))
            {
                var AssignPinCred = new AssignCredentialRequestModel
                {
                    Cardholder = cardholderGuid,
                    Credential = PinCredGuid
                };
                var AssignPinCredResult = await AssignCredentialToCardholder(AssignPinCred);
            }
            return Ok();
        }
        [HttpPost("Credential/CancelBookingAndCredentials")]
        public async Task<IActionResult> CancelBookingAndCredentials([FromBody] EntityDeleteBooking model)
        {
            string PinCredGuid = String.Empty;
            if (string.IsNullOrEmpty(model.PhysicalAccessCode) || string.IsNullOrEmpty(model.Action))
            {
                return BadRequest("Both credential PIN and Action are required.");
            }
            //Get PIN Credential GUID
            var GetPinCredentialResult = await FindCredentialByPin(model.PhysicalAccessCode);
            if (GetPinCredentialResult is OkObjectResult okResult && okResult.Value is ResponseModel PinCredResponse)
            {
                PinCredGuid = PinCredResponse?.Rsp?.Result?.Guid;
            }
            else
            {
                return StatusCode(500, $"Request error: Unable to get Credential with pin.");
            }
            //DeactivatePIN Credential
            if (!String.IsNullOrEmpty(PinCredGuid))
            {
                var DeactivateCredentialModel = new ActivateCredentialRequestModel
                {
                    Credential = PinCredGuid
                };
                var DeactivateCredentialResult = await DeactivateCredentialForNewBook(DeactivateCredentialModel);
            }
            return Ok();
        }
        [HttpPost("Credential/FindByPin")]
        public async Task<IActionResult> FindCredentialByPin([FromQuery] string credentialCode)
        {
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Construct the API request URL
            var requestUrl = $"{baseUrl}entity?q=entity=NewEntity(Credential),Format=KeypadCredentialFormat({Uri.EscapeDataString(credentialCode)}),Guid";

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
                        var PinCredResponse = JsonConvert.DeserializeObject<ResponseModel>(responseBody);
                        return Ok(PinCredResponse);
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
        [HttpPost("Credential/DeactivateCredentialForNewBook")]
        public async Task<IActionResult> DeactivateCredentialForNewBook([FromBody] ActivateCredentialRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Credential))
            {
                return BadRequest("Credential GUID is required.");
            }

            // Retrieve API credentials and base URL
            var (username, password, baseUrl) = GenetecApiHelper.GetApiCredentials(_configuration);

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // URL encode the credential parameter
            var encodedCredential = HttpUtility.UrlEncode($"entity={model.Credential}");
            var queryParams = $"{encodedCredential},Status.Deactivate()";

            // Construct the request URL
            var requestUrl = $"{baseUrl}entity?q={queryParams}";

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
                        return StatusCode((int)response.StatusCode, "Failed to deactivate the credential.");
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var PinCredResponse = JsonConvert.DeserializeObject<ResponseModel>(responseBody);

                    return Ok(PinCredResponse);
                }
                catch (HttpRequestException e)
                {
                    return StatusCode(500, $"Request error: {e.Message}");
                }
            }
        }

        //Create Credentials From NewBook API:
        [HttpPost("Credential/CreateCardholderAndCredentialForNewBook")]
        public async Task<IActionResult> CreateCardholderAndCredentialForNewBook([FromBody] EntityCredentials2 model)
        {
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.LicensePlate))
            {
                return BadRequest("Both credential name and license plate number are required.");
            }
            // Parse input dates as UTC
            DateTime activationDate = DateTime.SpecifyKind(
                DateTime.ParseExact(model.ActivationDateTime, "ddMMyy HHmm", CultureInfo.InvariantCulture),
                DateTimeKind.Utc
            );
            DateTime deactivationDate = DateTime.SpecifyKind(
                DateTime.ParseExact(model.DeactivationDateTime, "ddMMyy HHmm", CultureInfo.InvariantCulture),
                DateTimeKind.Utc
            );

            // Use Windows-compatible time zone ID
            string windowsTimeZoneId = "AUS Eastern Standard Time";
            TimeZoneInfo localZone;

            try
            {
                localZone = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                return StatusCode(500, $"Time zone ID '{windowsTimeZoneId}' not found on the server.");
            }
            catch (InvalidTimeZoneException)
            {
                return StatusCode(500, $"Time zone ID '{windowsTimeZoneId}' is invalid.");
            }

            // Convert UTC to local time
            DateTime activationLocal = TimeZoneInfo.ConvertTimeFromUtc(activationDate, localZone);
            DateTime deactivationLocal = TimeZoneInfo.ConvertTimeFromUtc(deactivationDate, localZone);

            var cardholderGuid = String.Empty;
            var licenseCredGuid = String.Empty;
            var pinCredGuid = String.Empty;
            var cardholderGroupGuid = "2cb29e64-f632-4795-bdcf-387051776812";
            //Create CardHolder
            var cardholder = new EntityCardholder
            {
                Name = model.Name,
                FirstName = model.Name,
                LastName = "",
                Email = "",
                MobilePhone = ""
            };
            // Call CreateCardholderFromNewBook and get the result
            var createCardholderResult = await _cardHolderController.CreateCardholderFromNewBook(cardholder);

            // Ensure the result is an OkObjectResult and contains the ResponseModel
            if (createCardholderResult is OkObjectResult okResult && okResult.Value is ResponseModel cardholderResponse)
            {
                cardholderGuid = cardholderResponse?.Rsp?.Result?.Guid;
            }
            else
            {
                return StatusCode(500, $"Request error: Unable to add Cardholder.");
            }
            //Add Activation and Deactivation Date
            if (deactivationDate > DateTime.MinValue && activationDate > DateTime.MinValue)
            {
                var AddCardholderActivationWithExpirationRequestModel = new CardholderActivationWithExpirationRequestModel
                {
                    Cardholder = cardholderGuid,
                    ActivationDate = activationDate,
                    ExpirationDate = deactivationDate
                };
                var SetActivationAndExpirationDatesForNewBookResult =
                    await _cardHolderController.SetActivationAndExpirationDatesForNewBook(AddCardholderActivationWithExpirationRequestModel);
            }

            //Assign Group to CardHolder
            if (!String.IsNullOrEmpty(cardholderGroupGuid) && !String.IsNullOrEmpty(cardholderGuid))
            {
                var AssignCardToGroup = new CardholderGroupRequestModel
                {
                    Cardholder = cardholderGuid,
                    Group = cardholderGroupGuid
                };
                var AssignCardToGroupResult = await _cardHolderGroupController.AddCardholderToGroupForNewbook(AssignCardToGroup);
            }
            //Create License Credential
            var LicenseCredential = new EntityCredentials
            {
                Name = model.Name,
                LicensePlate = model.LicensePlate
            };
            var CreateLicensePlateCredentialResult = await CreateLicensePlateCredentialForNewBook(LicenseCredential);
            // Ensure the result is an OkObjectResult and contains the ResponseModel
            if (CreateLicensePlateCredentialResult is OkObjectResult okResult2 && okResult2.Value is ResponseModel LicenseCredResponse)
            {
                licenseCredGuid = LicenseCredResponse?.Rsp?.Result?.Guid;
            }
            else
            {
                return StatusCode(500, $"Request error: Unable to create Credential with license plate.");
            }

            //Create Pin Credential
            var PinCredential = new CreateCredentialWithPinRequest
            {
                Name = model.Name,
                CredentialCode = model.Pin
            };
            var CreatePinCredentialResult = await CreateCredentialWithPinForNewBook(PinCredential);
            if (CreatePinCredentialResult is OkObjectResult okResult3 && okResult3.Value is ResponseModel PinCredResponse)
            {
                pinCredGuid = PinCredResponse?.Rsp?.Result?.Guid;
            }
            else
            {
                return StatusCode(500, $"Request error: Unable to add Credential with pin.");
            }
            //Assign License Credential to CardHolder
            if (!String.IsNullOrEmpty(licenseCredGuid))
            {
                var AssignLicenseCred = new AssignCredentialRequestModel
                {
                    Cardholder = cardholderGuid,
                    Credential = licenseCredGuid
                };
                var AssignLicenseCredResult = await AssignCredentialToCardholder(AssignLicenseCred);
            }
            //Assign Pin Credential to CardHolder
            if (!String.IsNullOrEmpty(pinCredGuid))
            {
                var AssignPinCred = new AssignCredentialRequestModel
                {
                    Cardholder = cardholderGuid,
                    Credential = pinCredGuid
                };
                var AssignPinCredResult = await AssignCredentialToCardholder(AssignPinCred);
            }
            return Ok(new
            {
                CardholderGuid = cardholderGuid,
                LicenseCredGuid = licenseCredGuid,
                PinCredGuid = pinCredGuid
            });
        }

        //API For Creating Custom Format Credential on Genetec with Pass Identifier
        [SkipCustomerAuthorization]
        [HttpPost("Credential/PostPassCreationTasks")]
        public async Task<IActionResult> PostPassCreationTasks([FromBody] PostPassCreationTaskModel entity)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var httpClient = new HttpClient();
            string CustomCredentialGuid = String.Empty;
            string LicenseCredGuid = String.Empty;
            string NewCardHolderGuid = String.Empty;

            var passDetails = await _passCreatorAPIController.GetPassDetailsForGenetec(entity.Identifier);
            
            // Assuming passDetails.PassData contains the string
            string passData = passDetails.PassData;

            //Get the Current Customer
            var Customer = await CredentialHelper.GetCustomerFromPassTemplateID(passDetails.PassTemplateGuid, httpClient);

            if (await dbContext.ProcessedAccessCodes.AnyAsync(a => a.PassIdentifier == passDetails.Identifier && a.CustomerId == Customer.CUSTOMER_ID))
                return StatusCode(500, $"Request error: This Pass is already processed.");

            //Validations
            if (Customer == null) { return StatusCode(500, $"Request error: Unable to find Customer details."); }
            if (!Customer.IsGeneticIntegration) { return StatusCode(500, $"Request error: Genetec is not Integrated for this Customer."); }
            
            var CustomCardFormatID = Customer.GenetecCustomCardFormatGUID; //Digital Pass W | Get From Customer
            //Check if Customer has NewBook Integration or not
            if (Customer.IsNewBookIntegration)
            {
                // Define a regex pattern to match "External System Passholder ID"
                string pattern = @"External System Passholder ID:\s*(\d+)";
                Match match = Regex.Match(passData, pattern);

                // Extract the value if a match is found
                string BookingID = match.Success ? match.Groups[1].Value : null;

                if (BookingID != null)
                {
                    //Get Booking Data
                    var BookingData = await dbContext.ProcessedAccessCodes.Where(b => b.BookingId == BookingID).FirstOrDefaultAsync();

                    if (BookingData != null)
                    {
                        if (Customer.IsGeneticIntegration)
                        {
                            //Create Custom Card Credential
                            var CustomCredentialData = new CreateDigitalPassCredentialRequest
                            {
                                FormatId = CustomCardFormatID,
                                Identifier = entity.Identifier,
                                Name = BookingData.BookingName

                            };
                            var CustomCredentialResult = await _genetecServices.CreateDigitalPassCredential(CustomCredentialData, Customer);
                            // Ensure the result is an OkObjectResult and contains the ResponseModel
                            if (CustomCredentialResult is ResponseModel CustomCredResponse)
                            {
                                CustomCredentialGuid = CustomCredResponse?.Rsp?.Result?.Guid;
                                //Assign the Custom Card Credential to Existing Card Holder
                                if (!String.IsNullOrEmpty(CustomCredentialGuid))
                                {
                                    var AssignLicenseCred = new AssignCredentialRequestModel
                                    {
                                        Cardholder = BookingData.CardholderGuid,
                                        Credential = CustomCredentialGuid
                                    };
                                    var AssignLicenseCredResult = await _genetecServices.AssignCredentialToCardholder(AssignLicenseCred, Customer);

                                    //Adding Activation & Expiration Date

                                    if (BookingData.AccessCodePeriodFrom > DateTime.MinValue && BookingData.AccessCodePeriodTo > DateTime.MinValue)
                                    {
                                        var activationRequest = new CardholderActivationWithExpirationRequestModel
                                        {
                                            Cardholder = CustomCredentialGuid,
                                            ActivationDate = BookingData.AccessCodePeriodFrom,
                                            ExpirationDate = BookingData.AccessCodePeriodTo
                                        };
                                        await _genetecServices.SetActivationAndExpirationDatesForNewBook(activationRequest, Customer);
                                    }
                                }
                                //Save Pass Created Status
                                var EntitiyBookingData = dbContext.ProcessedBookings.Where(b => b.BookingId == BookingID).FirstOrDefault();
                                if (EntitiyBookingData != null)
                                {
                                    EntitiyBookingData.PassClaimed = true;
                                    dbContext.ProcessedBookings.Update(EntitiyBookingData);
                                }
                                //Save Custom Credential GUID
                                BookingData.CustomCredGuid = CustomCredentialGuid;
                                BookingData.PassIdentifier = entity.Identifier;
                                dbContext.ProcessedAccessCodes.Update(BookingData);
                                await dbContext.SaveChangesAsync();
                            }
                            else
                            {
                                return StatusCode(500, $"Request error: Unable to create Custom Format Credential.");
                            }
                        }
                        else
                        {
                            // As Customer has No Genetec Integration So nothing Will Happen
                        }
                        return Ok();
                    }
                    else
                    {
                        return StatusCode(500, $"Booking Data not found in the provided data.");
                    }
                }
                else
                {
                    return StatusCode(500, $"Passholder ID not found in the provided data.");
                }
            }
            else
            {
                //NewBook Is not Integrated:
                //Create Credentials & Cardholder after pass Creation by taking data from Claim Page.

                string firstName = Regex.Match(passData, @"First Name:\s*(.+)")?.Groups[1].Value.Trim() ?? "";
                string lastName = Regex.Match(passData, @"Last Name:\s*(.+)")?.Groups[1].Value.Trim() ?? "";
                string email = Regex.Match(passData, @"Email address:\s*(.+)")?.Groups[1].Value.Trim() ?? "";
                string phone = Regex.Match(passData, @"Mobile Phone Number:\s*(.+)")?.Groups[1].Value.Trim() ?? "";
                string licensePlate = Regex.Match(passData, @"License Plates:\s*(.+)")?.Groups[1].Value.Trim() ?? "";

                string fullName = string.Join(" ", new[] { firstName, lastName }.Where(s => !string.IsNullOrWhiteSpace(s)));

                var CredentialActivationDate = string.IsNullOrWhiteSpace(passDetails.CreatedOn)
                      ? DateTime.MinValue.ToString("ddMMyyyy HHmm")
                      : DateTime.Parse(passDetails.CreatedOn).ToString("ddMMyyyy HHmm");

                var CredentialDeactivationDate = string.IsNullOrWhiteSpace(passDetails.ExpirationDate)
                      ? DateTime.MinValue.ToString("ddMMyyyy HHmm")
                      : DateTime.Parse(passDetails.ExpirationDate).ToString("ddMMyyyy HHmm");

                if (Customer.IsGeneticIntegration)
                {
                    var apiModel = new EntityCredentials3
                    {
                        Name = fullName,
                        Email = email,
                        Phone = phone,
                        LicensePlate = licensePlate,
                        ActivationDateTime = CredentialActivationDate,
                        DeactivationDateTime = CredentialDeactivationDate
                    };

                    GuidResponses responseData = await _genetecServices.CreateCardholderAndCredentialWithoutNewBookIntegration(apiModel, Customer);
                    var cardholderGuid = (string)responseData?.CardholderGuid;
                    var licenseCredGuid = (string)responseData?.LicenseCredGuid;


                    //Create Custom Card Credential
                    var CustomCredentialData = new CreateDigitalPassCredentialRequest
                    {
                        FormatId = CustomCardFormatID,
                        Identifier = entity.Identifier,
                        Name = fullName

                    };
                    var CustomCredentialResult = await _genetecServices.CreateDigitalPassCredential(CustomCredentialData, Customer);
                    if (CustomCredentialResult is ResponseModel CustomCredResponse)
                    {
                        CustomCredentialGuid = CustomCredResponse?.Rsp?.Result?.Guid;
                        //Assign the Custom Card Credential to Existing Card Holder
                        if (!String.IsNullOrEmpty(CustomCredentialGuid))
                        {
                            var AssignLicenseCred = new AssignCredentialRequestModel
                            {
                                Cardholder = cardholderGuid,
                                Credential = CustomCredentialGuid
                            };
                            var AssignLicenseCredResult = await _genetecServices.AssignCredentialToCardholder(AssignLicenseCred, Customer);

                            dbContext.ProcessedAccessCodes.Add(new ProcessedAccessCode
                            {
                                AccessCodeId = Guid.NewGuid().ToString("N"),
                                CustomerId = Customer.CUSTOMER_ID,
                                ProcessedAt = DateTime.UtcNow,
                                AccessCodeCarRego = licensePlate,
                                AccessCodePeriodFrom = DateTime.ParseExact(CredentialActivationDate, "ddMMyyyy HHmm", CultureInfo.InvariantCulture),
                                AccessCodePeriodTo = DateTime.ParseExact(CredentialDeactivationDate, "ddMMyyyy HHmm", CultureInfo.InvariantCulture),
                                SecurityAreaId = "0",
                                SecurityAreaName = "",
                                BookingId = "",
                                BookingName = fullName,
                                BookingArrival = DateTime.ParseExact(CredentialActivationDate, "ddMMyyyy HHmm", CultureInfo.InvariantCulture),
                                BookingDeparture = DateTime.ParseExact(CredentialDeactivationDate, "ddMMyyyy HHmm", CultureInfo.InvariantCulture),
                                GuestId = "",
                                GuestName = fullName,
                                CardholderGuid = cardholderGuid,
                                LicenseCredGuid = licenseCredGuid,
                                PinCredGuid = "",
                                CustomCredGuid = CustomCredentialGuid,
                                PassIdentifier = passDetails.Identifier
                            });

                            await dbContext.SaveChangesAsync();

                            //Adding Activation & Expiration Date
                            if (DateTime.ParseExact(CredentialActivationDate, "ddMMyyyy HHmm", CultureInfo.InvariantCulture) > DateTime.MinValue && DateTime.ParseExact(CredentialDeactivationDate, "ddMMyyyy HHmm", CultureInfo.InvariantCulture) > DateTime.MinValue)
                            {
                                var activationRequest = new CardholderActivationWithExpirationRequestModel
                                {
                                    Cardholder = CustomCredentialGuid,
                                    ActivationDate = DateTime.ParseExact(apiModel.ActivationDateTime, "ddMMyyyy HHmm", CultureInfo.InvariantCulture),
                                    ExpirationDate = DateTime.ParseExact(apiModel.DeactivationDateTime, "ddMMyyyy HHmm", CultureInfo.InvariantCulture)
                                };
                                await _genetecServices.SetActivationAndExpirationDatesForNewBook(activationRequest, Customer);
                            }

                        }

                    }
                }
                else
                {
                    //Since Genetec is not Integrated
                    dbContext.ProcessedAccessCodes.Add(new ProcessedAccessCode
                    {
                        AccessCodeId = Guid.NewGuid().ToString("N"),
                        CustomerId = Customer.CUSTOMER_ID,
                        ProcessedAt = DateTime.UtcNow,
                        AccessCodeCarRego = licensePlate,
                        AccessCodePeriodFrom = DateTime.ParseExact(CredentialActivationDate, "ddMMyyyy HHmm", CultureInfo.InvariantCulture),
                        AccessCodePeriodTo = DateTime.ParseExact(CredentialDeactivationDate, "ddMMyyyy HHmm", CultureInfo.InvariantCulture),
                        SecurityAreaId = "0",
                        SecurityAreaName = "",
                        BookingId = "",
                        BookingName = fullName,
                        BookingArrival = DateTime.ParseExact(CredentialActivationDate, "ddMMyyyy HHmm", CultureInfo.InvariantCulture),
                        BookingDeparture = DateTime.ParseExact(CredentialDeactivationDate, "ddMMyyyy HHmm", CultureInfo.InvariantCulture),
                        GuestId = "",
                        GuestName = fullName,
                        CardholderGuid = "",
                        LicenseCredGuid = "",
                        PinCredGuid = "",
                        CustomCredGuid = CustomCredentialGuid,
                        PassIdentifier = passDetails.Identifier
                    });

                    await dbContext.SaveChangesAsync();
                }
                
                return Ok();
            }
        }
        #endregion
    }
}
