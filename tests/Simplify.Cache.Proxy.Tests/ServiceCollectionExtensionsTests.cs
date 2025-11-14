using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using StackExchange.Redis;
using Simplify.Cache.Proxy;

namespace Simplify.Cache.Proxy.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddRedisCacheProxy_should_throw_when_services_are_null()
    {
        Action act = () => RedisCacheProxyServiceCollectionExtensions.AddRedisCacheProxy(null!, "localhost:6379");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddRedisCacheProxy_should_throw_when_connection_string_is_empty()
    {
        var services = new ServiceCollection();
        Action act = () => services.AddRedisCacheProxy(string.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddRedisCacheProxy_factory_should_register_singleton_connection()
    {
        var services = new ServiceCollection();
        var multiplexer = Substitute.For<IConnectionMultiplexer>();

        services.AddRedisCacheProxy(_ => multiplexer);
        var provider = services.BuildServiceProvider();

        var resolved1 = provider.GetRequiredService<IConnectionMultiplexer>();
        var resolved2 = provider.GetRequiredService<IConnectionMultiplexer>();

        resolved1.Should().BeSameAs(multiplexer);
        resolved2.Should().BeSameAs(multiplexer);
    }
}
