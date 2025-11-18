using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;
using UserService.Api.Exceptions;
using UserService.Api.Models;

namespace UserService.Api.Filters;

/// <summary>
/// Filter to validate that request bodies don't contain unknown properties
/// </summary>
public class RequestBodyValidationFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;

        if (request.ContentLength > 0 && request.ContentType?.Contains("application/json") == true)
        {
            request.EnableBuffering();
            request.Body.Position = 0;

            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var requestBody = reader.ReadToEndAsync().Result;
            request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(requestBody))
            {
                try
                {
                    using var document = JsonDocument.Parse(requestBody, new JsonDocumentOptions
                    {
                        AllowTrailingCommas = false
                    });

                    // Get the expected properties from the action parameter types
                    var actionParameters = context.ActionDescriptor.Parameters;
                    foreach (var parameter in actionParameters)
                    {
                        if (context.ActionArguments.TryGetValue(parameter.Name, out var argument) && argument != null)
                        {
                            var expectedProperties = argument.GetType()
                                .GetProperties()
                                .Select(p => char.ToLowerInvariant(p.Name[0]) + p.Name.Substring(1))
                                .ToHashSet();

                            if (document.RootElement.ValueKind == JsonValueKind.Object)
                            {
                                var actualProperties = document.RootElement
                                    .EnumerateObject()
                                    .Select(p => p.Name)
                                    .ToHashSet();

                                // Check for unknown properties
                                var unknownProperties = actualProperties.Except(expectedProperties).ToList();

                                if (unknownProperties.Any())
                                {
                                    var validationErrors = unknownProperties.Select(prop => new ValidationError
                                    {
                                        Field = prop,
                                        Message = $"Unknown property '{prop}' is not allowed in request body"
                                    }).ToList();

                                    throw new ValidationException(validationErrors);
                                }
                            }
                        }
                    }
                }
                catch (JsonException ex)
                {
                    throw new ValidationException(new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Field = "Request body",
                            Message = ex.Message
                        }
                    });
                }
            }
        }

        base.OnActionExecuting(context);
    }
}