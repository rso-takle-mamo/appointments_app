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
    builder.Services.AddSwaggerGen();
}
builder.Services.AddOpenApi();

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
    app.UseSwaggerUI();
}

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionHandler>();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();