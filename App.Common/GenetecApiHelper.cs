using Microsoft.Extensions.Configuration;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;


public static class GenetecApiHelper
{
    public class PassResponse
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("isSMS")]
        public bool IsSMS { get; set; }

        [JsonPropertyName("isEmail")]
        public bool IsEmail { get; set; }
    }
    // Method to get the username, password, and base URL from configuration
    public static (string Username, string Password, string BaseUrl) GetApiCredentials(IConfiguration configuration)
    {
        var username = configuration["GenetecAPI:Username"];
        var password = configuration["GenetecAPI:Password"];
        var baseUrl = configuration["GenetecAPI:BaseUrl"];

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(baseUrl))
        {
            throw new InvalidOperationException("API credentials or URL are missing.");
        }

        return (username, password, baseUrl);
    }

    // Method to encode username and password for Basic Auth
    public static string EncodeCredentials(string username, string password)
    {
        var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
        return Convert.ToBase64String(byteArray);
    }

    public static string ExtractGuidFromResponse(string responseBody)
    {
        try
        {
            // Load the response body as XML
            var xml = XDocument.Parse(responseBody);
            // Extract the Guid element's value
            return xml.Root.Element("Guid")?.Value;
        }
        catch
        {
            return null; // Handle parsing errors
        }
    }

    public static object ConvertXmlToJson(string xmlString)
    {
        var xml = System.Xml.Linq.XDocument.Parse(xmlString);
        var json = Newtonsoft.Json.JsonConvert.SerializeXNode(xml, Newtonsoft.Json.Formatting.None, true);
        return json;
    }

    public static async Task<PassResponse> GetPassUrlByNameAsync(string name)
    {
        // Define the endpoint URL
        string endpoint = "https://api-vrfid-id.azurewebsites.net/api/pass/PassURLByName";

        // Create the request body
        var requestBody = new { name };
        string jsonRequestBody = JsonSerializer.Serialize(requestBody);

        try
        {
            using (HttpClient client = new HttpClient())
            {
                // Set up the request content with JSON payload
                StringContent content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                // Send a GET request with content in the body
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, endpoint)
                {
                    Content = content
                };

                HttpResponseMessage response = await client.SendAsync(request);

                // Ensure the response indicates success
                response.EnsureSuccessStatusCode();

                // Read the response content as a string
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON response into the model
                var jsonDocument = JsonDocument.Parse(jsonResponse);
                var passResponse = new PassResponse
                {
                    Url = jsonDocument.RootElement.GetProperty("url").GetString(),
                    IsSMS = jsonDocument.RootElement.GetProperty("isSMS").GetBoolean(),
                    IsEmail = jsonDocument.RootElement.GetProperty("isEmail").GetBoolean(),
                };

                if (passResponse == null)
                {
                    throw new Exception("Failed to parse the response.");
                }

                return passResponse;
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions and optionally log them
            throw new Exception($"An error occurred while getting the pass response: {ex.Message}", ex);
        }
    }


}
public static class JsonElementExtensions
{
    public static JsonElement TryAddProperty(JsonElement element, string propertyName, object propertyValue)
    {
        var dictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(element.ToString());
        dictionary[propertyName] = propertyValue;
        return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(dictionary));
    }
}

