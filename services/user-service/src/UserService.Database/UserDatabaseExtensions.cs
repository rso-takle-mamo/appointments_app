using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Database.Repositories.Implementation;
using UserService.Database.Repositories.Interfaces;

namespace UserService.Database;

public static class UserDatabaseExtensions
{
    public static void AddUserDatabase(this IServiceCollection services)
    {
        services.AddDbContext<UserDbContext>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
    }
}