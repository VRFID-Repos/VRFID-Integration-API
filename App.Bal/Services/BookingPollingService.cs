using App.Bal.Services;
using App.Common.Model;
using App.Entity.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

public class BookingPollingService : BackgroundService
{
    private readonly ILogger<BookingPollingService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BookingProcessor _bookingProcessor;

    public BookingPollingService(
        ILogger<BookingPollingService> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        BookingProcessor bookingProcessor)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _bookingProcessor = bookingProcessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Booking Polling Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                
                var customers = await GetAllCustomersHavingNewBookIntegration();
                if (customers != null)
                {
                    foreach (var customer in customers)
                    {
                        if (customer == null || !customer.IsNewBookIntegration)
                            continue;

                        if (string.IsNullOrEmpty(customer.NewBookApiKey) ||
                            string.IsNullOrEmpty(customer.NewBookUsername) ||
                            string.IsNullOrEmpty(customer.NewBookPassword) ||
                            string.IsNullOrEmpty(customer.NewBookRegion) ||
                            string.IsNullOrEmpty(customer.NewBookEndpoint))
                        {
                            continue;
                        }
                        // Fetch bookings for each customer
                        await FetchBookingsForCustomer(customer, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during booking polling.");
            }

            // Wait before the next polling cycle
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); // Adjust as per your needs
        }
    }

    private async Task FetchBookingsForCustomer(CustomerDTO customer, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Fetching bookings for customer: {customer.CUSTOMER_FIRSTNAME}");

        try
        {
            if (string.IsNullOrWhiteSpace(customer.NewBookRegion) || string.IsNullOrWhiteSpace(customer.NewBookApiKey))
            {
                throw new ArgumentException("Region & API Key are required parameters.");
            }
            var client = _httpClientFactory.CreateClient();
            
            // Calculate the current UTC date for the entire day's range
            var currentUtcDate = DateTime.UtcNow.Date;
            var periodFrom = currentUtcDate.ToString("yyyy-MM-dd HH:mm:ss");
            var periodTo = currentUtcDate.AddDays(1).AddTicks(-1).ToString("yyyy-MM-dd HH:mm:ss");

            // Prepare request body
            var body = new Dictionary<string, object>
            {
                { "region", customer.NewBookRegion },
                { "api_key", customer.NewBookApiKey },
                { "list_type", "placed" },
                { "period_from", periodFrom },
                { "period_to", periodTo }
            };

            // Make API call
            var jsonBody = System.Text.Json.JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var apiUrl = $"{customer.NewBookEndpoint}bookings_list";
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{customer.NewBookUsername}:{customer.NewBookPassword}"));
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

            var response = await client.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to fetch bookings for customer {customer.CUSTOMER_FIRSTNAME}. StatusCode: {response.StatusCode}");
                return;
            }

            var responseData = await response.Content.ReadAsStringAsync();
            var bookingResponse = System.Text.Json.JsonSerializer.Deserialize<BookingResponse>(responseData);

            // Process the bookings
            await _bookingProcessor.ProcessBookingsAsync(bookingResponse.Data, customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching bookings for customer: {customer.CUSTOMER_FIRSTNAME}");
        }
    }
    private async Task FetchAccessCodesForCustomer(CustomerDTO customer, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Fetching access codes for customer: {customer.CUSTOMER_FIRSTNAME}");

        try
        {
            if (string.IsNullOrWhiteSpace(customer.NewBookRegion) || string.IsNullOrWhiteSpace(customer.NewBookApiKey))
            {
                throw new ArgumentException("Region & API Key are required parameters.");
            }
            var client = _httpClientFactory.CreateClient();
            // Calculate the date for the optional `created_when` parameter (if needed)
            var createdWhen = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");

            // Prepare request body
            var body = new Dictionary<string, object>
        {
            { "region", customer.NewBookRegion },
            { "api_key", customer.NewBookApiKey },
            { "created_when", createdWhen },
            { "access_code_mappings", true }
        };

            // Make API call
            var jsonBody = System.Text.Json.JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var apiUrl = $"{customer.NewBookEndpoint}access_codes_list";
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };

            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{customer.NewBookUsername}:{customer.NewBookPassword}"));
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

            var response = await client.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to fetch access codes for customer {customer.CUSTOMER_FIRSTNAME}. StatusCode: {response.StatusCode}");
                return;
            }

            var responseData = await response.Content.ReadAsStringAsync();
            var accessCodeResponse = System.Text.Json.JsonSerializer.Deserialize<AccessCodeResponse>(responseData);

            // Process the access codes
            if (accessCodeResponse?.Success == "true" && accessCodeResponse.Data != null)
            {
                await _bookingProcessor.ProcessAccessCodesAsync(accessCodeResponse.Data, customer);
            }
            else
            {
                _logger.LogWarning($"No valid access codes found for customer {customer.CUSTOMER_FIRSTNAME}.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching access codes for customer: {customer.CUSTOMER_FIRSTNAME}");
        }
    }

    private async Task<IEnumerable<CustomerDTO>> GetAllCustomersHavingNewBookIntegration()
    {
        // Use the injected IHttpClientFactory to create an HttpClient
        var httpClient = _httpClientFactory.CreateClient();

        var requestUrl = "http://api-vrfid-id.azurewebsites.net/api/GetAllCustomersHavingNewBookIntegration/";
        var response = await httpClient.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning($"Failed to fetch customer details. StatusCode: {response.StatusCode}");
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        var customers = JsonConvert.DeserializeObject<IEnumerable<CustomerDTO>>(content);

        if (customers == null)
        {
            _logger.LogWarning("No customers were returned by the API.");
            return null;
        }

        return customers;
    }

}
