using App.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Common.Extensions
{
    public class CustomerAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _headerName;

        public CustomerAuthorizeAttribute(string headerName)
        {
            _headerName = headerName;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check if the action or controller has Skip attribute
            var endpoint = context.HttpContext.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<SkipCustomerAuthorizationAttribute>() != null)
            {
                await next(); // Skip customer authorization
                return;
            }
            if (!context.HttpContext.Request.Headers.TryGetValue(_headerName, out var customerId))
            {
                context.Result = new UnauthorizedObjectResult("CustomerId header is missing.");
                return;
            }
            var customer = await GetCustomerDetailsFromService(context,customerId);
            if (customer == null)
            {
                context.Result = new UnauthorizedObjectResult("Invalid CustomerId.");
                return;
            }

            // Store customer details in HttpContext for later use
            context.HttpContext.Items["CustomerDetails"] = customer;
            await next();
        }
        private async Task<CustomerDTO> GetCustomerDetailsFromService(ActionExecutingContext context , string customerId)
        {
            var httpClient = context.HttpContext.RequestServices.GetService(typeof(HttpClient)) as HttpClient;

            if (httpClient == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return null;
            }

            var requestUrl = $"http://api-vrfid-id.azurewebsites.net/api/GetCustomerById/{customerId}";
            var response = await httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                context.Result = new BadRequestObjectResult("Invalid or Non-existent Customer ID.");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var customer = JsonConvert.DeserializeObject<CustomerDTO>(content);

            if (customer == null)
            {
                context.Result = new BadRequestObjectResult("Customer validation failed.");
                return null;
            }

            return customer;

        }
        //public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        //{
        //    if (!context.ActionArguments.ContainsKey(_customerIdParam))
        //    {
        //        context.Result = new BadRequestObjectResult("Customer ID is required.");
        //        return;
        //    }

        //    int customerId = (int)context.ActionArguments[_customerIdParam];
        //    var httpClient = context.HttpContext.RequestServices.GetService(typeof(HttpClient)) as HttpClient;

        //    if (httpClient == null)
        //    {
        //        context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
        //        return;
        //    }

        //    var requestUrl = $"http://api-vrfid-id.azurewebsites.net/api/GetCustomerById/{customerId}";
        //    var response = await httpClient.GetAsync(requestUrl);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        context.Result = new BadRequestObjectResult("Invalid or Non-existent Customer ID.");
        //        return;
        //    }

        //    var content = await response.Content.ReadAsStringAsync();
        //    var customer = JsonConvert.DeserializeObject<CustomerDTO>(content);

        //    if (customer == null)
        //    {
        //        context.Result = new BadRequestObjectResult("Customer validation failed.");
        //        return;
        //    }

        //    // Store the customer details in HttpContext for later use
        //    context.HttpContext.Items["CustomerDetails"] = customer;

        //    // Proceed to the next action if validation passes
        //    await next();
        //}
    }
}
