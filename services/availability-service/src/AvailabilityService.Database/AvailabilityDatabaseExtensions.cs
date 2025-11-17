using Microsoft.Extensions.DependencyInjection;
using AvailabilityService.Database.Repositories.Implementation;
using AvailabilityService.Database.Repositories.Interfaces;

namespace AvailabilityService.Database;

public static class AvailabilityDatabaseExtensions
{
    public static void AddAvailabilityDatabase(this IServiceCollection services)
    {
        services.AddDbContext<AvailabilityDbContext>();
        services.AddScoped<IWorkingHoursRepository, WorkingHoursRepository>();
        services.AddScoped<ITimeBlockRepository, TimeBlockRepository>();
        services.AddScoped<IGoogleCalendarIntegrationRepository, GoogleCalendarIntegrationRepository>();
        services.AddScoped<IBufferTimeRepository, BufferTimeRepository>();
        services.AddScoped<ITenantSettingsRepository, TenantSettingsRepository>();
    }
}