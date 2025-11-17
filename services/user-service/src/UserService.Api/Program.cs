using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using System.Text;
using System.Text.Json;
using UserService.Api.Middleware;
using UserService.Api.Services;
using UserService.Api.Filters;
using UserService.Api.Validators;
using UserService.Database;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ModelValidationFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Configure API behavior to suppress automatic model validation response
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

if (builder.Environment.IsDevelopment())
{
    // Add open api and swagger for development
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "UserService API",
            Version = "v1"
        });

        // Add JWT Authentication to Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header. Enter your JWT token below.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        // Add operation filter to handle security requirements per endpoint
        c.OperationFilter<SwaggerSecurityRequirementsOperationFilter>();
    });
}

builder.Configuration.AddEnvironmentVariables();

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? EnvironmentVariables.GetRequiredVariable("JWT_SECRET_KEY");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "UserService";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "UserService";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add database and health checks
builder.Services.AddUserDatabase();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () =>
    {
        try
        {
            return HealthCheckResult.Healthy("UserService is running");
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

// Register filters
builder.Services.AddScoped<ModelValidationFilter>();

// Register application services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService.Api.Services.UserService>();
builder.Services.AddScoped<ITenantService, TenantService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API v1");
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

app.UseAuthentication();
app.UseAuthorization();

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