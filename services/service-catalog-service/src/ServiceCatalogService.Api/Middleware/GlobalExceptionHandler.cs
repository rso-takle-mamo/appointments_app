using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ServiceCatalogService.Api.Models;
using ServiceCatalogService.Api.Exceptions;

namespace ServiceCatalogService.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var errorResponse = CreateErrorResponse(exception);
            var statusCode = GetStatusCode(exception);

            logger.LogError(
                "Exception: {ExceptionType}, Message: {ExceptionMessage}, StatusCode: {StatusCode}",
                exception.GetType().Name,
                exception.Message,
                statusCode);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var responseJson = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(responseJson);
        }
    }

    private static object CreateErrorResponse(Exception exception)
    {
        return exception switch
        {
            // Custom domain exceptions
            ValidationException ex => ErrorResponses.CreateValidation(
                $"Validation failed with {ex.ValidationErrors?.Count ?? 0} error(s).",
                ex.ValidationErrors ?? new List<ValidationError>()
            ),
            NotFoundException ex => ErrorResponses.Create(
                ex.ErrorCode,
                ex.Message,
                ex.ResourceType ?? "Resource",
                ex.ResourceId
            ),
            ConflictException ex => ErrorResponses.Create(ex.ErrorCode, ex.Message),
            AuthenticationException ex => ErrorResponses.Create(ex.ErrorCode, ex.Message),
            AuthorizationException ex => ErrorResponses.Create(ex.ErrorCode, ex.Message),
            DatabaseOperationException ex => ErrorResponses.Create(ex.ErrorCode, ex.Message),

            // Database exceptions
            DbUpdateException ex => ErrorResponses.Create(
                "DATABASE_ERROR",
                ex.InnerException?.Message ?? ex.Message
            ),

            // Built-in exceptions
            KeyNotFoundException ex => ErrorResponses.Create("NOT_FOUND", ex.Message),
            ArgumentException ex => ErrorResponses.Create("VALIDATION_ERROR", ex.Message),
            InvalidOperationException ex => ErrorResponses.Create("BUSINESS_RULE_VIOLATION", ex.Message),
            UnauthorizedAccessException ex => ErrorResponses.Create("UNAUTHORIZED", "Access denied"),

            _ => ErrorResponses.Create("INTERNAL_SERVER_ERROR", "An internal server error occurred.")
        };
    }

    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            // Custom domain exceptions
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            AuthenticationException => StatusCodes.Status401Unauthorized,
            AuthorizationException => StatusCodes.Status403Forbidden,
            DatabaseOperationException => StatusCodes.Status500InternalServerError,

            // Database exceptions
            DbUpdateException => StatusCodes.Status400BadRequest,

            // Built-in exceptions
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status409Conflict,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,

            _ => StatusCodes.Status500InternalServerError
        };
}