using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Simplify.Cache.Proxy;
using Simplify.Cache.Proxy.Attributes;
using StackExchange.Redis;
using Xunit;

namespace Simplify.Cache.Proxy.Tests;

public sealed class RedisCachingProxyTests
{
    [Fact]
    public async Task Should_cache_expression_based_results_and_replay_from_redis()
    {
        using var fixture = RepositoryFixture<ISampleRepository, SampleRepository>.Create();
        var repository = fixture.Proxy;
        var implementation = fixture.Implementation;

        var first = await repository.FilterAsync(static c => c.City == "Lisbon");
        var second = await repository.FilterAsync(static c => c.City == "Lisbon");

        first.Should().HaveCount(1);
        second.Should().BeEquivalentTo(first);
        implementation.ExpressionCalls.Should().Be(1);
        fixture.Redis.Storage.Count.Should().Be(1);
    }

    [Fact]
    public async Task Should_bypass_cache_when_expression_changes()
    {
        using var fixture = RepositoryFixture<ISampleRepository, SampleRepository>.Create();
        var repository = fixture.Proxy;
        var implementation = fixture.Implementation;

        await repository.FilterAsync(static c => c.City == "Lisbon");
        await repository.FilterAsync(static c => c.City == "Porto");

        implementation.ExpressionCalls.Should().Be(2);
        fixture.Redis.Storage.Keys.Should().HaveCount(2);
    }

    [Fact]
    public async Task Should_cache_parameter_based_results()
    {
        using var fixture = RepositoryFixture<ISampleRepository, SampleRepository>.Create();
        var repository = fixture.Proxy;
        var implementation = fixture.Implementation;

        var first = await repository.CountByCityAsync("Lisbon");
        var second = await repository.CountByCityAsync("Lisbon");

        first.Should().Be(1);
        second.Should().Be(first);
        implementation.ParameterCalls.Should().Be(1);
    }

    [Fact]
    public async Task Attribute_expiration_should_drive_cache_ttl()
    {
        using var fixture = RepositoryFixture<ISampleRepository, SampleRepository>.Create();
        var repository = fixture.Proxy;

        _ = await repository.CountByCityAsync("Lisbon");

        fixture.Redis.Expirations.Single().Value.Should().Be(TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Default_expiration_should_apply_when_attribute_is_missing()
    {
        using var fixture = RepositoryFixture<IPlainRepository, PlainRepository>.Create(TimeSpan.FromMinutes(5));
        var repository = fixture.Proxy;

        _ = await repository.GetAsync("value");

        fixture.Redis.Expirations.Single().Value.Should().Be(TimeSpan.FromMinutes(5));
    }
}

internal interface ISampleRepository
{
    Task<IReadOnlyList<TestCustomer>> FilterAsync(Expression<Func<TestCustomer, bool>> predicate);
    Task<int> CountByCityAsync(string city);
}

[RedisCached(1)]
internal sealed class SampleRepository : ISampleRepository
{
    private static readonly IReadOnlyList<TestCustomer> Customers =
    [
        new("Fabio", "Lisbon"),
        new("Ana", "Porto")
    ];

    public int ExpressionCalls { get; private set; }
    public int ParameterCalls { get; private set; }

    public Task<IReadOnlyList<TestCustomer>> FilterAsync(Expression<Func<TestCustomer, bool>> predicate)
    {
        ExpressionCalls++;
        var result = Customers.AsQueryable().Where(predicate).ToList();
        return Task.FromResult<IReadOnlyList<TestCustomer>>(result);
    }

    public Task<int> CountByCityAsync(string city)
    {
        ParameterCalls++;
        var count = Customers.Count(customer =>
            string.Equals(customer.City, city, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(count);
    }
}

internal interface IPlainRepository
{
    Task<string> GetAsync(string value);
}

internal sealed class PlainRepository : IPlainRepository
{
    public Task<string> GetAsync(string value) => Task.FromResult(value);
}

internal sealed record TestCustomer(string Name, string City);

internal sealed class RepositoryFixture<TInterface, TImplementation> : IDisposable
    where TInterface : class
    where TImplementation : class, TInterface
{
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;

    private RepositoryFixture(ServiceProvider provider, IServiceScope scope, FakeRedis redis)
    {
        _provider = provider;
        _scope = scope;
        Redis = redis;
    }

    public TInterface Proxy => _scope.ServiceProvider.GetRequiredService<TInterface>();
    public TImplementation Implementation => _scope.ServiceProvider.GetRequiredService<TImplementation>();
    public FakeRedis Redis { get; }

    public static RepositoryFixture<TInterface, TImplementation> Create(TimeSpan? defaultExpiration = null)
    {
        var redis = FakeRedis.Create();

        var multiplexer = Substitute.For<IConnectionMultiplexer>();
        multiplexer.GetDatabase(Arg.Any<int>(), Arg.Any<object?>()).Returns(redis.Database);

        var services = new ServiceCollection();
        services.AddSingleton(multiplexer);
        services.AddRedisCachedRepository<TInterface, TImplementation>(defaultExpiration);

        var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        var scope = provider.CreateScope();
        return new RepositoryFixture<TInterface, TImplementation>((ServiceProvider)provider, scope, redis);
    }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
    }
}

internal sealed class FakeRedis
{
    private readonly Dictionary<string, RedisValue> _storage;
    private readonly Dictionary<string, TimeSpan?> _expirations;

    private FakeRedis(IDatabase database, Dictionary<string, RedisValue> storage, Dictionary<string, TimeSpan?> expirations)
    {
        Database = database;
        _storage = storage;
        _expirations = expirations;
    }

    public IDatabase Database { get; }
    public IReadOnlyDictionary<string, RedisValue> Storage => _storage;
    public IReadOnlyDictionary<string, TimeSpan?> Expirations => _expirations;

    public static FakeRedis Create()
    {
        var storage = new Dictionary<string, RedisValue>(StringComparer.Ordinal);
        var expirations = new Dictionary<string, TimeSpan?>(StringComparer.Ordinal);
        var database = Substitute.For<IDatabase>();

        database
            .StringGet(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Returns(call => storage.TryGetValue(call.Arg<RedisKey>().ToString(), out var value) ? value : RedisValue.Null);

        database
            .StringGetAsync(Arg.Any<RedisKey>(), Arg.Any<CommandFlags>())
            .Returns(call => Task.FromResult(storage.TryGetValue(call.Arg<RedisKey>().ToString(), out var value) ? value : RedisValue.Null));

        database
            .StringSet(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan?>(), Arg.Any<bool>(), Arg.Any<When>(), Arg.Any<CommandFlags>())
            .Returns(call =>
            {
                var key = call.Arg<RedisKey>().ToString();
                storage[key] = call.Arg<RedisValue>();
                expirations[key] = call.Arg<TimeSpan?>();
                return true;
            });

        database
            .StringSetAsync(Arg.Any<RedisKey>(), Arg.Any<RedisValue>(), Arg.Any<TimeSpan?>(), Arg.Any<bool>(), Arg.Any<When>(), Arg.Any<CommandFlags>())
            .Returns(call =>
            {
                var key = call.Arg<RedisKey>().ToString();
                storage[key] = call.Arg<RedisValue>();
                expirations[key] = call.Arg<TimeSpan?>();
                return Task.FromResult(true);
            });

        return new FakeRedis(database, storage, expirations);
    }
}
