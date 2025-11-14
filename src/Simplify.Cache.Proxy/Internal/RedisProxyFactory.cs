using System.Reflection;
using StackExchange.Redis;

namespace Simplify.Cache.Proxy.Internal;

internal static class RedisProxyFactory
{
    public static TInterface CreateWithRedisCache<TInterface, TImplementation>(
        TImplementation target,
        IDatabase redis,
        TimeSpan? expiration)
        where TInterface : class
    {
        if (target is null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        if (redis is null)
        {
            throw new ArgumentNullException(nameof(redis));
        }

        var proxy = DispatchProxy.Create<TInterface, RedisCachingProxy<TImplementation>>();
        var redisProxy = (RedisCachingProxy<TImplementation>)(object)proxy!;
        redisProxy.Configure(target, redis, expiration);

        return proxy!;
    }
}
