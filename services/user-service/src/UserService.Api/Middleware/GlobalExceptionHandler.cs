using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using UserService.Api.Models;

namespace UserService.Api.Middleware;

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
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            logger.LogError(
                "Error trace ID: {TraceId}, Type: {ErrorType}, Message: {Message}",
                traceId,
                exception.GetType().Name,
                exception.Message);

            context.Response.StatusCode = GetStatusCode(exception);
            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }

    private static ErrorResponse CreateErrorResponse(Exception exception)
    {
        return exception switch
        {
            InvalidOperationException ex => new ErrorResponse
            {
                Error = new Error
                {
                    Code = "CONFLICT",
                    Message = ex.Message
                }
            },
            ArgumentException ex => new ErrorResponse
            {
                Error = new Error
                {
                    Code = "INVALID_ARGUMENT",
                    Message = ex.Message
                }
            },
            DbUpdateException ex => new ErrorResponse
            {
                Error = new Error
                {
                    Code = "DATABASE_ERROR",
                    Message = ex.InnerException?.Message ?? ex.Message
                }
            },
            KeyNotFoundException ex => new ErrorResponse
            {
                Error = new Error
                {
                    Code = "NOT_FOUND",
                    Message = ex.Message
                }
            },
            UnauthorizedAccessException ex => new ErrorResponse
            {
                Error = new Error
                {
                    Code = "UNAUTHORIZED",
                    Message = ex.Message
                }
            },
            _ => new ErrorResponse
            {
                Error = new Error
                {
                    Code = "INTERNAL_SERVER_ERROR",
                    Message = "An internal server error occurred."
                }
            }
        };
    }

    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            InvalidOperationException => StatusCodes.Status409Conflict,
            ArgumentException => StatusCodes.Status400BadRequest,
            DbUpdateException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
}