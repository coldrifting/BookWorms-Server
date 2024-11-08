using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApplication1.Swagger;

// Makes padlock icon only shows on endpoints needing authentication
// ReSharper disable once ClassNeverInstantiated.Global
public class AuthenticationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        IList<object> actionMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        bool isAuthorized = actionMetadata.Any(metadataItem => metadataItem is AuthorizeAttribute);
        bool allowAnonymous = actionMetadata.Any(metadataItem => metadataItem is AllowAnonymousAttribute);

        if (!isAuthorized || allowAnonymous)
        {
            return;
        }
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Security = new List<OpenApiSecurityRequirement>();

        var item = new OpenApiSecurityRequirement()
        { { new()
        {                            
            Reference = new()
            {                  
                Id = "Bearer",             
                Type = ReferenceType.SecurityScheme
            }
        }, Array.Empty<string>() } };
        operation.Security.Add(item
        );
    }
}