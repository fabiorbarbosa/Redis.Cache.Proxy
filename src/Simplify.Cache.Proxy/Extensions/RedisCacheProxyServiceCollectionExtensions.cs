using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Simplify.Cache.Proxy.Attributes;
using Simplify.Cache.Proxy.Internal;
using StackExchange.Redis;

namespace Simplify.Cache.Proxy;

/// <summary>
/// Service collection helpers to wire Redis caching proxies through DI.
/// </summary>
public static class RedisCacheProxyServiceCollectionExtensions
{
    /// <summary>
    /// Registers an <see cref="IConnectionMultiplexer"/> using the provided connection string.
    /// </summary>
    /// <param name="services">DI container to update.</param>
    /// <param name="connectionString">Redis connection string.</param>
    public static IServiceCollection AddRedisCacheProxy(this IServiceCollection services, string connectionString)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionString));
        return services;
    }

    /// <summary>
    /// Registers an <see cref="IConnectionMultiplexer"/> using a custom factory.
    /// </summary>
    /// <param name="services">DI container to update.</param>
    /// <param name="connectionFactory">Factory responsible for creating the connection.</param>
    public static IServiceCollection AddRedisCacheProxy(this IServiceCollection services, Func<IServiceProvider, IConnectionMultiplexer> connectionFactory)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (connectionFactory is null)
        {
            throw new ArgumentNullException(nameof(connectionFactory));
        }

        services.AddSingleton(connectionFactory);
        return services;
    }

    /// <summary>
    /// Wraps the implementation of <typeparamref name="TImplementation"/> with a Redis DispatchProxy and exposes it as <typeparamref name="TInterface"/>.
    /// </summary>
    /// <typeparam name="TInterface">Interface contract to expose.</typeparam>
    /// <typeparam name="TImplementation">Concrete repository to decorate.</typeparam>
    /// <param name="services">DI container to update.</param>
    /// <param name="defaultExpiration">Optional fallback expiration when the attribute is not present.</param>
    public static IServiceCollection AddRedisCachedRepository<TInterface, TImplementation>(
        this IServiceCollection services,
        TimeSpan? defaultExpiration = null)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (!typeof(TInterface).IsInterface)
        {
            throw new ArgumentException($"{typeof(TInterface).FullName} must be an interface.");
        }

        services.AddScoped<TImplementation>();
        services.AddScoped(provider =>
        {
            var implementation = provider.GetRequiredService<TImplementation>();
            var multiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
            var database = multiplexer.GetDatabase();

            var attrExpiration = typeof(TImplementation)
                .GetCustomAttribute<RedisCachedAttribute>()
                ?.Expiration;

            var expiration = attrExpiration ?? defaultExpiration;

            return RedisProxyFactory.CreateWithRedisCache<TInterface, TImplementation>(
                implementation,
                database,
                expiration);
        });

        return services;
    }
}
