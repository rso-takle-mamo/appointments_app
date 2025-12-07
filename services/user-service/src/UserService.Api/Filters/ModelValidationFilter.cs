using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Runtime.CompilerServices;
using UserService.Api.Exceptions;
using UserService.Api.Models;

namespace UserService.Api.Filters;

public class ModelValidationFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            // Group validation errors by field
            var groupedValidationErrors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    // Key: Normalize field name to camelCase
                    x => GetNormalizedFieldName(x.Key),
                    // Value: List of validation errors for that field
                    x => x.Value!.Errors.Select(error => new ValidationError
                    {
                        Field = GetNormalizedFieldName(x.Key),
                        Message = error.ErrorMessage
                    }).ToList()
                );

            throw new ValidationException($"Validation failed with {groupedValidationErrors.Count} field(s).", groupedValidationErrors);
        }

        base.OnActionExecuting(context);
    }

    private static string GetNormalizedFieldName(string fieldName)
    {
        // Handle empty field name (request body errors)
        if (string.IsNullOrEmpty(fieldName))
            return "requestBody";

        // Convert PascalCase to camelCase (e.g., "Username" to "username")
        if (char.IsUpper(fieldName[0]))
        {
            return char.ToLowerInvariant(fieldName[0]) + fieldName[1..];
        }

        // Return as-is if already lowercase or mixed case
        return fieldName.ToLowerInvariant();
    }
}