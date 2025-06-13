using System.Reflection;
using StackExchange.Redis;

namespace Redis.Cache.Proxy.CrossCutting.Cache;

internal static class RedisProxyFactory
{
    public static TInterface CreateWithRedisCache<TInterface>(TInterface target,
        IDatabase redis,
        TimeSpan? expiration = null)
        where TInterface : class
    {
        var proxy = DispatchProxy.Create<TInterface, RedisCachingProxy<TInterface>>();
        var redisProxy = (RedisCachingProxy<TInterface>)(object)proxy!;
        redisProxy.Configure(target, redis, expiration);
        return proxy;
    }
}