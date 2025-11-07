using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceCatalogService.Api.Models;

namespace ServiceCatalogService.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.Clear();
        context.Response.ContentType = "application/json";

        var errorResponse = new ErrorResponse();

        switch (exception)
        {
            case InvalidOperationException ex:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse.Error.Code = "BUSINESS_RULE_VIOLATION";
                errorResponse.Error.Message = ex.Message;
                break;

            case ArgumentException ex:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Error.Code = "INVALID_ARGUMENT";
                errorResponse.Error.Message = ex.Message;
                break;

            case DbUpdateException ex:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Error.Code = "DATABASE_ERROR";
                errorResponse.Error.Message = "Database operation failed. Please check your input.";
                break;

            case KeyNotFoundException ex:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Error.Code = "RESOURCE_NOT_FOUND";
                errorResponse.Error.Message = ex.Message;
                break;

            case UnauthorizedAccessException ex:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Error.Code = "UNAUTHORIZED";
                errorResponse.Error.Message = "Access denied";
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Error.Code = "INTERNAL_SERVER_ERROR";
                errorResponse.Error.Message = "An unexpected error occurred. Please try again later.";
                break;
        }

        errorResponse.Error.TraceId = context.TraceIdentifier;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }
}