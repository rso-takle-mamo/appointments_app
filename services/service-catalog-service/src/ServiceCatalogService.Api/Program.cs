using Microsoft.OpenApi.Models;
using ServiceCatalogService.Api.Middleware;
using ServiceCatalogService.Database;

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
            Title = "ServiceCatalog API",
            Version = "v1"
        });
    });
}

// Database configuration
builder.Services.AddServiceCatalogDatabase();

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceCatalog API v1");
        c.RoutePrefix = "swagger";
    });
}

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionHandler>();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();