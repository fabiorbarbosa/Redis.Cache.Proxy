using System.Reflection;

using StackExchange.Redis;

namespace Redis.Cache.Proxy.CrossCutting.Cache;

internal static class RedisProxyFactory
{
    public static TInterface CreateWithRedisCache<TInterface, TImplementation>(
        TImplementation target,
        IDatabase redis,
        TimeSpan? expiration = null)
        where TInterface : class
    {
        var proxy = DispatchProxy.Create<TInterface, RedisCachingProxy<TImplementation>>();
        var redisProxy = (RedisCachingProxy<TImplementation>)(object)proxy!;
        redisProxy.Configure(target, redis, expiration);
        return proxy;
    }
}