using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks;

public static class HealthApiExtensions
{
    public static void RegisterHealthServices(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database health check");
    }
    
    public static void AddHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/startup");
        endpoints.MapHealthChecks("/healthz");
        endpoints.MapHealthChecks("/ready");
    }
}