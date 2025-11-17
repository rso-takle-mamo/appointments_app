using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using AvailabilityService.Api.Middleware;
using AvailabilityService.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    // Add open api and swagger for development
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Availability API",
            Version = "v1"
        });
    });
}

// Database configuration
builder.Services.AddAvailabilityDatabase();

// Health checks configuration
builder.Services.AddHealthChecks()
    .AddCheck("self", () =>
    {
        try
        {
            return HealthCheckResult.Healthy("Service is running");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Service check failed", ex);
        }
    }, tags: ["self"])
    .AddNpgSql(
        connectionString: EnvironmentVariables.GetRequiredVariable("DATABASE_CONNECTION_STRING"),
        healthQuery: "SELECT 1;",
        name: "postgresql",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["db", "postgresql"]);

// Register middleware
builder.Services.AddTransient<GlobalExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Availability API v1");
        c.RoutePrefix = "swagger";
        // Hide the black topbar
        c.HeadContent =
        """
            <style>
                .swagger-ui .topbar {
                    display: none;
                }
            </style>
         """;
    });
}

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionHandler>();

app.UseHttpsRedirection();

app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    AllowCachingResponses = false
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = (check) => check.Tags.Contains("self"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    AllowCachingResponses = false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = (check) => check.Tags.Contains("self") || check.Tags.Contains("db"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    AllowCachingResponses = false
});

app.Run();