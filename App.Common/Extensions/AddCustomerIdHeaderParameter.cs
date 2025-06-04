using App.Common.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

public class AddCustomerIdHeaderParameter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        // Check for SkipCustomerAuthorization attribute
        var hasSkipAttribute = context.MethodInfo
            .DeclaringType
            .GetCustomAttributes(true)
            .OfType<SkipCustomerAuthorizationAttribute>()
            .Any()
            ||
            context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<SkipCustomerAuthorizationAttribute>()
            .Any();

        // If the attribute is present, skip adding the header
        if (hasSkipAttribute)
            return;

        // Add the "customerId" header parameter if not skipped
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "customerid",
            In = ParameterLocation.Header,
            Description = "Customer ID for authorization",
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }
}
