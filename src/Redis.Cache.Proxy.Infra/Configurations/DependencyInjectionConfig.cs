using Microsoft.Extensions.DependencyInjection;
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
        services.AddRedis("localhost");

        // Register repository with Redis caching proxy
        services.AddRedisCachedRepository<ICustomerRepository, CustomerRepository>();
    }
}
