using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        // TODO actually implement health check
        return Task.FromResult(HealthCheckResult.Healthy());
    }
}