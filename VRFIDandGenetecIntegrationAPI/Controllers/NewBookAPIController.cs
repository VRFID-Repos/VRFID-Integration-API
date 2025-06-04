using App.Common.Model;
using App.Common;
using App.Entity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static GenetecApiHelper;
using static JsonElementExtensions;
using System.Drawing;

namespace VRFIDandGenetecIntegrationAPI.Controllers
{
    [ApiController]
    public class NewBookAPIController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        // Inject IConfiguration via constructor
        public NewBookAPIController(IConfiguration configuration, HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _httpClient = httpClient;   
            _serviceProvider = serviceProvider;
        }

        //[HttpGet("NewBook/GetBookingsList")]
        //public async Task<IActionResult> GetBookingsList([FromQuery] string listType)
        //{
        //    // Fetching credentials and API details from appsettings.json
        //    var apiConfig = _configuration.GetSection("NewBook");
        //    var apiKey = apiConfig["ApiKey"];
        //    var username = apiConfig["Username"];
        //    var password = apiConfig["Password"];
        //    var region = apiConfig["region"];
        //    var endpoint = apiConfig["Endpoint"] + "bookings_list";

        //    // Prepare Basic Auth header
        //    var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

        //    // Prepare request body
        //    var requestBody = new
        //    {
        //        region = region,
        //        api_key = apiKey,
        //        list_type = listType ?? "inhouse" // Default to "inhouse" if not provided
        //    };

        //    using (var client = new HttpClient())
        //    {
        //        // Prepare HTTP request
        //        var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
        //        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
        //        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        // Convert request body to JSON
        //        string jsonContent = JsonConvert.SerializeObject(requestBody);
        //        httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        //        try
        //        {
        //            // Send request
        //            var response = await client.SendAsync(httpRequest);
        //            var responseBody = await response.Content.ReadAsStringAsync();
        //            var options = new JsonSerializerOptions
        //            {
        //                PropertyNameCaseInsensitive = true
        //            };

        //            BookingResponse Bookingresponse = System.Text.Json.JsonSerializer.Deserialize<BookingResponse>(responseBody, options);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                // Return success response
        //                return Ok(Bookingresponse);
        //            }
        //            else
        //            {
        //                // Return error response
        //                return StatusCode((int)response.StatusCode, responseBody);
        //            }
        //        }
        //        catch (HttpRequestException e)
        //        {
        //            // Return error for request failure
        //            return StatusCode(500, $"Request error: {e.Message}");
        //        }
        //    }
        //}
        [HttpGet("NewBook/GetBookingsList")]
        public async Task<IActionResult> GetBookingList(
                                                        [FromQuery] DateTime? periodFrom,
                                                        [FromQuery] DateTime? periodTo,
                                                        [FromQuery] string listType = "inhouse",
                                                        [FromQuery] int dataOffset = 0,
                                                        [FromQuery] int dataLimit = 100)
        {
            try
            {
                // Validate data_limit
                if (dataLimit > 1000)
                {
                    return BadRequest(new { Message = "data_limit cannot exceed 1000." });
                }

                // Retrieve customer details from HttpContext
                var customer = HttpContext.Items["CustomerDetails"] as CustomerDTO;

                if (customer == null)
                {
                    return Unauthorized("Customer details not found.");
                }
                // Get NewBook credentials
                var newbookCredentials = CredentialHelper.GetNewBookCredentials(customer);

                if (newbookCredentials == null)
                    return BadRequest("NewBook credentials not found or integration not enabled.");
                // Get API credentials and base URL
                var (apiKey, apiUsername, apiPassword, region, apiEndpoint) = newbookCredentials.Value;

                // Build the request body
                var body = new Dictionary<string, object>
                {
                    { "region", region },
                    { "api_key", apiKey },
                    { "list_type", string.IsNullOrEmpty(listType) ? "inhouse" : listType },
                    { "data_offset", dataOffset },
                    { "data_limit", dataLimit }
                };

                // Add optional parameters dynamically
                if (periodFrom.HasValue)
                    body.Add("period_from", periodFrom.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                if (periodTo.HasValue)
                    body.Add("period_to", periodTo.Value.ToString("yyyy-MM-dd HH:mm:ss"));

                // Serialize the request body to JSON
                var jsonBody = System.Text.Json.JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                // Prepare the HTTP request
                var apiUrl = $"{apiEndpoint}bookings_list";
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                // Add Basic Authentication
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUsername}:{apiPassword}"));
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                // Send the HTTP request
                var response = await _httpClient.SendAsync(httpRequest);

                // Ensure successful status code
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize and return the response content
                var responseData = await response.Content.ReadAsStringAsync();
                var deserializedResponse = System.Text.Json.JsonSerializer.Deserialize<BookingResponse>(responseData);

                // Extract desired details
                var bookingDetails = deserializedResponse.Data.Select(b => new
                {
                    BookingId = b.BookingId,
                    BookingFrom = b.BookingArrival,
                    BookingTo = b.BookingDeparture,
                    Guests = b.Guests.Select(g => new
                    {
                        GuestId = g.Id,
                        Street = g.Street,
                        City = g.city,
                        State = g.state,
                        Postcode = g.postcode,
                        Country = g.country,
                        Email = g.ContactDetails.FirstOrDefault(c => c.Type == "email")?.Content,
                        Phone = g.ContactDetails.FirstOrDefault(c => c.Type == "phone")?.Content,
                        Equipments = g.Equipments
                    }),
                    SiteName = b.SiteName,
                    AccessCodes = b.AccessCodes
                });

                // Create pagination metadata
                var pagination = new
                {
                    dataOffset,
                    dataLimit,
                    dataCount = deserializedResponse?.DataCount ?? 0,
                    dataTotal = deserializedResponse?.DataTotal ?? 0
                };

                // Return response with pagination metadata
                return Ok(new { Pagination = pagination, Data = bookingDetails });
            }
            catch (Exception ex)
            {
                // Return error details in case of exceptions
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }
        [HttpGet("NewBook/GetBookingsListV2")]
        public async Task<IActionResult> GetBookingListV2(
                                                        [FromQuery] DateTime? periodFrom,
                                                        [FromQuery] DateTime? periodTo,
                                                        [FromQuery] string listType = "inhouse",
                                                        [FromQuery] int dataOffset = 0,
                                                        [FromQuery] int dataLimit = 100)
        {
            try
            {
                // Validate data_limit
                if (dataLimit > 1000)
                {
                    return BadRequest(new { Message = "data_limit cannot exceed 1000." });
                }
                List<int> bookingIds = new List<int>();
                
                // Retrieve customer details from HttpContext
                var customer = HttpContext.Items["CustomerDetails"] as CustomerDTO;

                if (customer == null)
                {
                    return Unauthorized("Customer details not found.");
                }
                // Get NewBook credentials
                var newbookCredentials = CredentialHelper.GetNewBookCredentials(customer);

                if (newbookCredentials == null)
                    return BadRequest("NewBook credentials not found or integration not enabled.");
                // Get API credentials and base URL
                var (apiKey, apiUsername, apiPassword, region, apiEndpoint) = newbookCredentials.Value;


                // Build the request body
                var body = new Dictionary<string, object>
                {
                    { "region", region },
                    { "api_key", apiKey },
                    { "list_type", string.IsNullOrEmpty(listType) ? "inhouse" : listType },
                    { "data_offset", dataOffset },
                    { "data_limit", dataLimit }
                };

                // Add optional parameters dynamically
                if (periodFrom.HasValue)
                    body.Add("period_from", periodFrom.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                if (periodTo.HasValue)
                    body.Add("period_to", periodTo.Value.ToString("yyyy-MM-dd HH:mm:ss"));

                // Serialize the request body to JSON
                var jsonBody = System.Text.Json.JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                // Prepare the HTTP request
                var apiUrl = $"{apiEndpoint}bookings_list";
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                // Add Basic Authentication
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUsername}:{apiPassword}"));
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                // Send the HTTP request
                var response = await _httpClient.SendAsync(httpRequest);

                // Ensure successful status code
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize and return the response content
                var responseData = await response.Content.ReadAsStringAsync();
                var deserializedResponse = System.Text.Json.JsonSerializer.Deserialize<object>(responseData);
                
                var deserializedResponse2 = System.Text.Json.JsonSerializer.Deserialize<BookingResponse2>(responseData);

                if (deserializedResponse2 != null)
                {
                    // Extract booking IDs
                    foreach (var booking in deserializedResponse2.Data)
                    {
                        if (booking.TryGetProperty("booking_id", out JsonElement bookingIdElement))
                        {
                            int bookingId = bookingIdElement.GetInt32();
                            bookingIds.Add(bookingId);
                            //var emailResponse = GetBookingEmailInfo(bookingId.ToString());
                        }
                        else
                        {
                            Console.WriteLine("Booking ID not found.");
                        }
                        
                    }
                }

                // Return response with pagination metadata
                return Ok(deserializedResponse);
            }
            catch (Exception ex)
            {
                // Return error details in case of exceptions
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }
        [HttpGet("NewBook/GetBookingsListV3")]
        public async Task<IActionResult> GetBookingListV3(
                                                        [FromQuery] DateTime? periodFrom,
                                                        [FromQuery] DateTime? periodTo,
                                                        [FromQuery] string listType = "inhouse",
                                                        [FromQuery] int dataOffset = 0,
                                                        [FromQuery] int dataLimit = 100)
        {
            try
            {
                // Validate data_limit
                if (dataLimit > 1000)
                {
                    return BadRequest(new { Message = "data_limit cannot exceed 1000." });
                }
                // Retrieve customer details from HttpContext
                var customer = HttpContext.Items["CustomerDetails"] as CustomerDTO;

                if (customer == null)
                {
                    return Unauthorized("Customer details not found.");
                }
                // Get NewBook credentials
                var newbookCredentials = CredentialHelper.GetNewBookCredentials(customer);

                if (newbookCredentials == null)
                    return BadRequest("NewBook credentials not found or integration not enabled.");
                // Get API credentials and base URL
                var (apiKey, apiUsername, apiPassword, region, apiEndpoint) = newbookCredentials.Value;


                // Build the request body
                var body = new Dictionary<string, object>
                {
                    { "region", region },
                    { "api_key", apiKey },
                    { "list_type", string.IsNullOrEmpty(listType) ? "inhouse" : listType },
                    { "data_offset", dataOffset },
                    { "data_limit", dataLimit }
                };

                if (periodFrom.HasValue)
                    body.Add("period_from", periodFrom.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                if (periodTo.HasValue)
                    body.Add("period_to", periodTo.Value.ToString("yyyy-MM-dd HH:mm:ss"));

                // Serialize the request body
                var jsonBody = System.Text.Json.JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                // Prepare the HTTP request
                var apiUrl = $"{apiEndpoint}bookings_list";
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                // Add Basic Authentication
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUsername}:{apiPassword}"));
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                // Send the HTTP request
                var response = await _httpClient.SendAsync(httpRequest);

                // Ensure successful status code
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }

                // Deserialize the response content
                var responseData = await response.Content.ReadAsStringAsync();
                var deserializedResponse = System.Text.Json.JsonSerializer.Deserialize<BookingResponse2>(responseData);

                if (deserializedResponse == null || deserializedResponse.Data == null)
                {
                    return Ok(new { Message = "No bookings found." });
                }

                // Enhance bookings with email status
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Enhance bookings with email status
                for (int i = 0; i < deserializedResponse.Data.Count; i++)
                {
                    var booking = deserializedResponse.Data[i];

                    if (booking.TryGetProperty("booking_id", out JsonElement bookingIdElement))
                    {
                        var bookingId = bookingIdElement.GetInt32().ToString();
                        var entityBookingData = await dbContext.ProcessedBookings
                            .Where(b => b.BookingId == bookingId)
                            .FirstOrDefaultAsync();

                        // Add email status to the booking data
                        var updatedBooking = entityBookingData != null
                            ? JsonElementExtensions.TryAddProperty(booking, "email_status", new
                            {
                                email_scheduled = entityBookingData.EmailScheduled,
                                email_sent = entityBookingData.EmailSent,
                                pass_claimed = entityBookingData.PassClaimed
                            })
                            : JsonElementExtensions.TryAddProperty(booking, "email_status", new
                            {
                                email_scheduled = false,
                                email_sent = false,
                                pass_claimed = false
                            });

                        // Replace the original booking in the list
                        deserializedResponse.Data[i] = updatedBooking;
                    }
                }

                // Return enhanced response
                return Ok(deserializedResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpGet("NewBook/GetBookingEmailInfo")]
        public async Task<IActionResult> GetBookingEmailInfo(string BookingID)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var EntitiyBookingData = await dbContext.ProcessedBookings.Where(b => b.BookingId == BookingID).FirstOrDefaultAsync();  
            if (EntitiyBookingData != null)
            {
                var bookingResponse = new BookingEmailStatus
                {
                    EmailScheduled = EntitiyBookingData.EmailScheduled,
                    EmailSent = EntitiyBookingData.EmailSent,
                    PassClaimed = EntitiyBookingData.PassClaimed,
                };
                return Ok(bookingResponse);
            }
            var falseResponse = new BookingEmailStatus
            {
                EmailScheduled = false,
                EmailSent = false,
                PassClaimed = false,
            };
            return Ok(falseResponse);
           
        }
       [HttpPost("NewBook/UpdateBooking")]
        public async Task<IActionResult> UpdateBooking([FromBody] BookingUpdateRequest bookingUpdateRequest)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(bookingUpdateRequest.BookingId))
                {
                    return BadRequest(new { Message = "Booking ID is required." });
                }

                // Retrieve customer details from HttpContext
                var customer = HttpContext.Items["CustomerDetails"] as CustomerDTO;

                if (customer == null)
                {
                    return Unauthorized("Customer details not found.");
                }
                // Get NewBook credentials
                var newbookCredentials = CredentialHelper.GetNewBookCredentials(customer);

                if (newbookCredentials == null)
                    return BadRequest("Genetec credentials not found or integration not enabled.");
                // Get API credentials and base URL
                var (apiKey, apiUsername, apiPassword, region, apiEndpoint) = newbookCredentials.Value;


                // Build the request payload
                var requestPayload = new
                {
                    region,
                    api_key = apiKey,
                    booking_id = bookingUpdateRequest.BookingId,
                    access_codes = bookingUpdateRequest.AccessCodes
                };

                var jsonBody = System.Text.Json.JsonSerializer.Serialize(requestPayload);

                // Prepare the HTTP request
                var apiUrl = $"{apiEndpoint}bookings_update";
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                // Add Basic Authentication
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiUsername}:{apiPassword}"));
                httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                // Send the HTTP request
                var response = await _httpClient.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { Message = errorContent });
                }

                // Return the response
                var responseData = await response.Content.ReadAsStringAsync();
                return Ok(System.Text.Json.JsonSerializer.Deserialize<object>(responseData));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }

    }
}
