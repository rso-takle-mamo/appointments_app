using HealthChecks;
using UserService.Requests;
using UserService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.RegisterHealthServices();
builder.Services.AddControllers();
builder.Services.AddScoped<ValidModelFilter>();
builder.Services.AddScoped<UserRepository>();

var app = builder.Build();

app.AddHealthEndpoints();
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();