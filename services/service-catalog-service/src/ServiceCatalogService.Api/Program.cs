using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using HealthChecks.UI.Client;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ServiceCatalogService.Api.Middleware;
using ServiceCatalogService.Api.Services;
using ServiceCatalogService.Api.Services.Interfaces;
using ServiceCatalogService.Api.Filters;
using ServiceCatalogService.Api.Configuration;
using ServiceCatalogService.Database;
using Microsoft.AspNetCore.Mvc;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ModelValidationFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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
            Title = "ServiceCatalog API",
            Version = "v1"
        });

        // Configure enum handling for Swagger
        c.SchemaFilter<EnumSchemaFilter>();

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

        c.OperationFilter<AuthorizeOperationFilter>();

        // Include XML Comments in Swagger
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        }
        c.EnableAnnotations();
    });
}

builder.Configuration.AddEnvironmentVariables();

// Configure JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
var jwtKey = !string.IsNullOrEmpty(jwtSettings.Key) ? jwtSettings.Key : EnvironmentVariables.GetRequiredVariable("JWT_SECRET_KEY");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

// Database configuration
builder.Services.AddServiceCatalogDatabase();

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

// Register filters
builder.Services.AddScoped<ModelValidationFilter>();

// Register application services
builder.Services.AddScoped<IUserContextService, UserContextService>();

// Register business services
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceCatalog API v1");
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

// Add /metrics endpoints for Prometheus
app.UseHttpMetrics(); 
app.MapMetrics();

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