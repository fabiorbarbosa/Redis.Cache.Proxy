using Microsoft.Extensions.DependencyInjection;

using Redis.Cache.Proxy.Application.Customers;
using Redis.Cache.Proxy.Data.Contexts;
using Redis.Cache.Proxy.Data.Repositories;

namespace Redis.Cache.Proxy.Infra.Configurations;

public static class DependencyInjectionConfig
{
    /// <summary>
    /// Configures the dependency injection for the application.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    public static void AddDependencies(this IServiceCollection services)
    {
        // Add Redis connection
        services.AddRedis("localhost:6379");

        // Add Database context
        services.AddScoped<CacheDbContext>();

        // Add Application services
        services.AddScoped<ICustomerService, CustomerService>();

        // Register repository with Redis caching proxy
        services.AddRedisCachedRepository<ICustomerRepository, CustomerRepository>();
    }
}
