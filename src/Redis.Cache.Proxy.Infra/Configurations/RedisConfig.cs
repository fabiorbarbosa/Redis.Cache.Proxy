using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Redis.Cache.Proxy.CrossCutting.Cache;
using StackExchange.Redis;
using Redis.Cache.Proxy.CrossCutting.Shared.Attributes;

namespace Redis.Cache.Proxy.Infra.Configurations;

internal static class RedisConfig
{
    /// <summary>
    /// Adds Redis connection to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The Redis connection string.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRedis(this IServiceCollection services, string connectionString)
    {
        // builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect("localhost"));

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString));
        return services;
    }

    /// <summary>
    /// Registers a repository with Redis caching using a DispatchProxy.
    /// </summary>
    /// <typeparam name="TInterface">The interface type of the repository.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the repository.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRedisCachedRepository<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddScoped<TImplementation>();
        services.AddScoped(provider =>
        {
            if (!typeof(TInterface).IsInterface)
                throw new ArgumentException($"Type {typeof(TInterface).FullName} must be an interface.");

            var redis = provider.GetRequiredService<IConnectionMultiplexer>() ?? throw new InvalidOperationException("Could not resolve Redis connection.");
            var db = redis.GetDatabase() ?? throw new InvalidOperationException("Could not resolve Redis database.");
            var implementation = provider.GetRequiredService<TImplementation>() ?? throw new InvalidOperationException($"Could not resolve implementation of {typeof(TImplementation).FullName}");
            var attrImpl = typeof(TImplementation).GetCustomAttribute<RedisCachedAttribute>() ?? throw new InvalidOperationException($"Type {typeof(TImplementation).FullName} must have a RedisCachedAttribute.");
            var proxy = DispatchProxy.Create<TInterface, RedisCachingProxy<TInterface>>();

            RedisProxyFactory.CreateWithRedisCache(implementation, db, attrImpl.Expiration);

            return proxy;
        });

        return services;
    }
}
