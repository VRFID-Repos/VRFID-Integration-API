using App.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Common
{
    public static class CredentialHelper
    {
        
        public static (string ApiKey, string Username, string Password, string Region, string Endpoint)? GetNewBookCredentials(CustomerDTO customer)
        {
            if (customer == null || !customer.IsNewBookIntegration)
                return null;

            if (string.IsNullOrEmpty(customer.NewBookApiKey) ||
                string.IsNullOrEmpty(customer.NewBookUsername) ||
                string.IsNullOrEmpty(customer.NewBookPassword) ||
                string.IsNullOrEmpty(customer.NewBookRegion) ||
                string.IsNullOrEmpty(customer.NewBookEndpoint))
            {
                throw new InvalidOperationException("Missing NewBook credentials for the customer.");
            }

            return (customer.NewBookApiKey, customer.NewBookUsername, customer.NewBookPassword, customer.NewBookRegion, customer.NewBookEndpoint);
        }
        public static (string Username, string Password, string BaseUrl)? GetGenetecCredentials(CustomerDTO customer)
        {
            if (customer == null || !customer.IsGeneticIntegration)
                return null;

            if (string.IsNullOrEmpty(customer.GeneticUsername) ||
                string.IsNullOrEmpty(customer.GeneticPassword) ||
                string.IsNullOrEmpty(customer.GeneticBaseUrl))
            {
                throw new InvalidOperationException("Missing Genetec credentials for the customer.");
            }

            return (customer.GeneticUsername, customer.GeneticPassword, customer.GeneticBaseUrl);
        }
        public static async Task<CustomerDTO> GetCustomerFromPassTemplateID(string passTemplateID, HttpClient httpClient)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient), "HttpClient cannot be null");

            var requestUrl = $"http://api-vrfid-id.azurewebsites.net/api/pass/CustomerByPassTypeGUID/{passTemplateID}";

            var response = await httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to fetch customer. Status Code: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<CustomerDTO>(content);

            if (customer == null)
            {
                throw new Exception("Customer validation failed.");
            }

            return customer;
        }
    }

}
