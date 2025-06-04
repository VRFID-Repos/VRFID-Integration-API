using App.Bal.Services;
using App.Common;
using App.Common.Messages;
using App.Common.Model;
using App.Entity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace App.Bal.Repositories
{
    public class GenetecServices : IGenetecServices
    {
        private readonly IConfiguration _configuration;
        public GenetecServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ResponseModel> CreateCardholderFromNewBook(EntityCardholder cardholder, CustomerDTO customer)
        {
            // Get NewBook credentials
            var genetecCredentials = CredentialHelper.GetGenetecCredentials(customer);

            if (genetecCredentials == null)
                return null;

            // Get API credentials and base URL
            var (username, password, baseUrl) = genetecCredentials.Value;

            if (string.IsNullOrEmpty(cardholder.Name))
            {
                return null;
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
                        return null;
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var cardholderResponse = JsonConvert.DeserializeObject<ResponseModel>(responseBody);

                    return cardholderResponse; // Return the parsed JSON response directly
                }
                catch (HttpRequestException e)
                {
                    return null;
                }
            }
        }
        public async Task<String> SetActivationAndExpirationDatesForNewBook(CardholderActivationWithExpirationRequestModel model, CustomerDTO customer)
        {
            if (string.IsNullOrEmpty(model.Cardholder))
            {
                return null;
            }

            if (model.ActivationDate == default)
            {
                return null;
            }

            if (model.ExpirationDate == default)
            {
                return null;
            }

            // Get API credentials and base URL
            var genetecCredentials = CredentialHelper.GetGenetecCredentials(customer);

            if (genetecCredentials == null)
                return null;

            // Get API credentials and base URL
            var (username, password, baseUrl) = genetecCredentials.Value;

            // Encode the credentials for Basic Auth
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Format the already-AEST dates
            var activationDateString = model.ActivationDate.ToString("yyyy-MM-ddTHH:mm:sszzz"); // Retain AEST offset
            var expirationDateString = model.ExpirationDate.ToString("yyyy-MM-ddTHH:mm:sszzz"); // Retain AEST offset

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
                        return null;
                    }

                    // Read the response content
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Return the JSON response directly
                    return responseBody;
                }
                catch (HttpRequestException e)
                {
                    return null;
                }
            }
        }
        public async Task<String> AddCardholderToGroupForNewbook(CardholderGroupRequestModel model, CustomerDTO customer)
        {
            if (string.IsNullOrEmpty(model.Group) || string.IsNullOrEmpty(model.Cardholder))
            {
                return null;
            }

            var genetecCredentials = CredentialHelper.GetGenetecCredentials(customer);

            if (genetecCredentials == null)
                return null;

            // Get API credentials and base URL
            var (username, password, baseUrl) = genetecCredentials.Value;

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
                        return null;
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();

                    return responseBody;
                }
                catch (HttpRequestException e)
                {
                    return null;
                }
            }
        }
        public async Task<ResponseModel> CreateLicensePlateCredentialForNewBook(EntityCredentials model, CustomerDTO customer)
        {
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.LicensePlate))
            {
                return null;
            }
            // Retrieve API credentials and base URL
            var genetecCredentials = CredentialHelper.GetGenetecCredentials(customer);

            if (genetecCredentials == null)
                return null;

            // Get API credentials and base URL
            var (username, password, baseUrl) = genetecCredentials.Value;

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
                        return null;
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var LicenseCredResponse = JsonConvert.DeserializeObject<ResponseModel>(responseBody);

                    return LicenseCredResponse; // Return the parsed JSON response directly
                }
                catch (HttpRequestException e)
                {
                    return null;
                }
            }
        }
        public async Task<ResponseModel> CreateCredentialWithPinForNewBook([FromBody] CreateCredentialWithPinRequest request, CustomerDTO customer)
        {
            var genetecCredentials = CredentialHelper.GetGenetecCredentials(customer);

            if (genetecCredentials == null)
                return null;

            // Get API credentials and base URL
            var (username, password, baseUrl) = genetecCredentials.Value;
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
                        return null;
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var PinCredResponse = JsonConvert.DeserializeObject<ResponseModel>(responseBody);

                    return PinCredResponse; // Return the parsed JSON response directly
                }
                catch (HttpRequestException e)
                {
                    return null;
                }
            }
        }
        public async Task<String> AssignCredentialToCardholder([FromBody] AssignCredentialRequestModel model, CustomerDTO customer)
        {
            if (string.IsNullOrEmpty(model.Cardholder) || string.IsNullOrEmpty(model.Credential))
            {
                return null;
            }

            // Retrieve API credentials and base URL
            var genetecCredentials = CredentialHelper.GetGenetecCredentials(customer);

            if (genetecCredentials == null)
                return null;

            // Get API credentials and base URL
            var (username, password, baseUrl) = genetecCredentials.Value;

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
                        return null;
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
                catch (HttpRequestException e)
                {
                    return null;
                }
            }
        }

        public async Task<GuidResponses> CreateCardholderAndCredentialForNewBook([FromBody] EntityCredentials2 model, CustomerDTO customer)
        {
            
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.LicensePlate))
            {
                return null;
            }
            // Parse input dates as UTC
            DateTime activationDate =
                DateTime.ParseExact(model.ActivationDateTime, "ddMMyy HHmm", CultureInfo.InvariantCulture);

            DateTime deactivationDate =
                DateTime.ParseExact(model.DeactivationDateTime, "ddMMyy HHmm", CultureInfo.InvariantCulture);

            // Convert UTC to local time
            //DateTime activationLocal = TimeZoneInfo.ConvertTimeToUtc(activationDate, localZone);
            //DateTime deactivationLocal = TimeZoneInfo.ConvertTimeToUtc(deactivationDate, localZone);

            DateTime activationLocal = activationDate;
            DateTime deactivationLocal = deactivationDate;

            string cardholderGuid = string.Empty;
            string licenseCredGuid = string.Empty;
            string pinCredGuid = string.Empty;
            string cardholderGroupGuid = customer.GenetecCustomerGroupGUID;

            // Create Cardholder
            var cardholder = new EntityCardholder
            {
                Name = model.Name,
                FirstName = model.Name,
                LastName = "",
                Email = "",
                MobilePhone = ""
            };

            var cardholderResponse = await CreateCardholderFromNewBook(cardholder, customer);
            if (cardholderResponse?.Rsp?.Result?.Guid == null)
            {
                return null;
            }
            cardholderGuid = cardholderResponse.Rsp.Result.Guid;

            // Assign Cardholder to Group
            if (!string.IsNullOrEmpty(cardholderGroupGuid))
            {
                var groupRequest = new CardholderGroupRequestModel
                {
                    Cardholder = cardholderGuid,
                    Group = cardholderGroupGuid
                };
                await AddCardholderToGroupForNewbook(groupRequest, customer);
            }

            // Create License Plate Credential
            var licenseCredential = new EntityCredentials
            {
                Name = model.Name,
                LicensePlate = model.LicensePlate
            };
            var licenseCredResponse = await CreateLicensePlateCredentialForNewBook(licenseCredential, customer);
            if (licenseCredResponse?.Rsp?.Result?.Guid == null)
            {
                return null;
            }
            licenseCredGuid = licenseCredResponse.Rsp.Result.Guid;
            
            // Set Activation and Expiration Date in License Plate Credential
            if (activationLocal > DateTime.MinValue && deactivationLocal > DateTime.MinValue)
            {
                var activationRequest = new CardholderActivationWithExpirationRequestModel
                {
                    Cardholder = licenseCredGuid,
                    ActivationDate = activationLocal,
                    ExpirationDate = deactivationLocal
                };
                await SetActivationAndExpirationDatesForNewBook(activationRequest, customer);
            }
            // Create PIN Credential
            var pinCredentialRequest = new CreateCredentialWithPinRequest
            {
                Name = model.Name,
                CredentialCode = model.Pin
            };
            var pinCredResponse = await CreateCredentialWithPinForNewBook(pinCredentialRequest, customer);
            if (pinCredResponse?.Rsp?.Result?.Guid == null)
            {
                return null;
            }
            pinCredGuid = pinCredResponse.Rsp.Result.Guid;

            // Set Activation and Expiration Date in  PIN Credential
            if (activationLocal > DateTime.MinValue && deactivationLocal > DateTime.MinValue)
            {
                var activationRequest = new CardholderActivationWithExpirationRequestModel
                {
                    Cardholder = pinCredGuid,
                    ActivationDate = activationLocal,
                    ExpirationDate = deactivationLocal
                };
                await SetActivationAndExpirationDatesForNewBook(activationRequest, customer);
            }

            // Assign Credentials to Cardholder
            if (!string.IsNullOrEmpty(licenseCredGuid))
            {
                var assignLicenseRequest = new AssignCredentialRequestModel
                {
                    Cardholder = cardholderGuid,
                    Credential = licenseCredGuid
                };
                await AssignCredentialToCardholder(assignLicenseRequest, customer);
            }

            if (!string.IsNullOrEmpty(pinCredGuid))
            {
                var assignPinRequest = new AssignCredentialRequestModel
                {
                    Cardholder = cardholderGuid,
                    Credential = pinCredGuid
                };
                await AssignCredentialToCardholder(assignPinRequest, customer);
            }
            GuidResponses guidResponse = new GuidResponses()
            {
                CardholderGuid = cardholderGuid,
                LicenseCredGuid = licenseCredGuid,
                PinCredGuid = pinCredGuid
            };
            return guidResponse;
        }

        public async Task<ResponseModel> CreateDigitalPassCredential([FromBody] CreateDigitalPassCredentialRequest request, CustomerDTO customer)
        {
            // Retrieve API credentials and base URL
            var genetecCredentials = CredentialHelper.GetGenetecCredentials(customer);

            if (genetecCredentials == null)
                return null;

            // Get API credentials and base URL
            var (username, password, baseUrl) = genetecCredentials.Value;
            var base64Credentials = GenetecApiHelper.EncodeCredentials(username, password);

            // Remove delimiters and validate identifier
            var identifier = request.Identifier.Replace("-", "").ToUpper();
            if (identifier.Length != 32)
            {
                return null;
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

                        return CredResponse; // Return the parsed JSON response directly
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (HttpRequestException e)
                {
                    return null;
                }
            }
        }
        public async Task<GuidResponses> CreateCardholderAndCredentialWithoutNewBookIntegration([FromBody] EntityCredentials3 model, CustomerDTO customer)
        {

            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.LicensePlate))
            {
                return null;
            }
            if (customer == null) { return null; }
            // Parse input dates as UTC
            DateTime activationDate = DateTime.MinValue;
            DateTime deactivationDate = DateTime.MinValue;

            if (!string.IsNullOrWhiteSpace(model.ActivationDateTime))
            {
                activationDate = DateTime.ParseExact(model.ActivationDateTime, "ddMMyyyy HHmm", CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrWhiteSpace(model.DeactivationDateTime))
            {
                deactivationDate = DateTime.ParseExact(model.DeactivationDateTime, "ddMMyyyy HHmm", CultureInfo.InvariantCulture);
            }

            DateTime activationLocal = activationDate;
            DateTime deactivationLocal = deactivationDate;

            string cardholderGuid = string.Empty;
            string licenseCredGuid = string.Empty;
            string pinCredGuid = string.Empty;
            string cardholderGroupGuid = string.Empty;
            cardholderGroupGuid = customer.GenetecCustomerGroupGUID;

            // Create Cardholder
            var cardholder = new EntityCardholder
            {
                Name = model.Name,
                FirstName = model.Name,
                LastName = "",
                Email = model.Email,
                MobilePhone = model.Phone
            };

            var cardholderResponse = await CreateCardholderFromNewBook(cardholder, customer);
            if (cardholderResponse?.Rsp?.Result?.Guid == null)
            {
                return null;
            }
            cardholderGuid = cardholderResponse.Rsp.Result.Guid;

            // Assign Cardholder to Group
            if (!string.IsNullOrEmpty(cardholderGroupGuid))
            {
                var groupRequest = new CardholderGroupRequestModel
                {
                    Cardholder = cardholderGuid,
                    Group = cardholderGroupGuid
                };
                await AddCardholderToGroupForNewbook(groupRequest, customer);
            }
            if (!string.IsNullOrEmpty(model.LicensePlate))
            {
                // Create License Plate Credential
                var licenseCredential = new EntityCredentials
                {
                    Name = model.Name,
                    LicensePlate = model.LicensePlate
                };
                var licenseCredResponse = await CreateLicensePlateCredentialForNewBook(licenseCredential, customer);
                if (licenseCredResponse?.Rsp?.Result?.Guid == null)
                {
                    return null;
                }
                licenseCredGuid = licenseCredResponse.Rsp.Result.Guid;

                // Set Activation and Expiration Date in License Plate Credential
                if (activationLocal > DateTime.MinValue && deactivationLocal > DateTime.MinValue)
                {
                    var activationRequest = new CardholderActivationWithExpirationRequestModel
                    {
                        Cardholder = licenseCredGuid,
                        ActivationDate = activationLocal,
                        ExpirationDate = deactivationLocal
                    };
                    await SetActivationAndExpirationDatesForNewBook(activationRequest, customer);
                }

                // Assign Credentials to Cardholder
                if (!string.IsNullOrEmpty(licenseCredGuid))
                {
                    var assignLicenseRequest = new AssignCredentialRequestModel
                    {
                        Cardholder = cardholderGuid,
                        Credential = licenseCredGuid
                    };
                    await AssignCredentialToCardholder(assignLicenseRequest, customer);
                }
            }

            GuidResponses guidResponse = new GuidResponses()
            {
                CardholderGuid = cardholderGuid,
                LicenseCredGuid = licenseCredGuid,
                PinCredGuid = pinCredGuid
            };
            return guidResponse;
        }

    }
}
