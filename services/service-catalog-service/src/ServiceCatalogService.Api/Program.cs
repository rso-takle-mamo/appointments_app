using Microsoft.EntityFrameworkCore;
using ServiceCatalogService.Api.Data;
using ServiceCatalogService.Api.Interfaces;
using ServiceCatalogService.Api.Middleware;
using ServiceCatalogService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Database configuration
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("DATABASE_CONNECTION_STRING environment variable is not set");
    Environment.Exit(1);
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register business services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IServiceService, ServiceService>();

// Register middleware
builder.Services.AddTransient<GlobalExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionHandler>();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();