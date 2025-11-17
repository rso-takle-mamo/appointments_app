using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UserService.Api.Filters;

public class SwaggerSecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var authorizeAttributes = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .ToList();

        if (authorizeAttributes.Any())
        {
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            };
        }
        else
        {
            operation.Security = new List<OpenApiSecurityRequirement>();
        }
    }
}