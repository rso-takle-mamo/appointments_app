using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceCatalogService.Database.Repositories.Implementation;
using ServiceCatalogService.Database.Repositories.Interfaces;

namespace ServiceCatalogService.Database;

public static class ServiceCatalogDatabaseExtensions
{
    public static void AddServiceCatalogDatabase(this IServiceCollection services)
    {
        services.AddDbContext<ServiceCatalogDbContext>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
    }
}