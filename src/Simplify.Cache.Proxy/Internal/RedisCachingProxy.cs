using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using StackExchange.Redis;

namespace Simplify.Cache.Proxy.Internal;

internal class RedisCachingProxy<TImplementation> : DispatchProxy
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    private TImplementation _decorated = default!;
    private IDatabase _redis = default!;
    private TimeSpan _expiration = DefaultExpiration;

    public void Configure(TImplementation decorated, IDatabase redis, TimeSpan? expiration)
    {
        if (decorated is null)
        {
            throw new ArgumentNullException(nameof(decorated));
        }

        if (redis is null)
        {
            throw new ArgumentNullException(nameof(redis));
        }

        if (expiration.HasValue && expiration.Value < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(expiration), "Expiration must be non-negative.");
        }

        _decorated = decorated;
        _redis = redis;
        _expiration = expiration ?? DefaultExpiration;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod is null)
        {
            throw new ArgumentNullException(nameof(targetMethod));
        }

        if (targetMethod.ReturnType == typeof(Task))
        {
            var method = typeof(RedisCachingProxy<TImplementation>)
                .GetMethod(nameof(InvokeAsyncWithoutResult), BindingFlags.NonPublic | BindingFlags.Instance);

            return method?.Invoke(this, [targetMethod, args]);
        }

        if (targetMethod.ReturnType.IsGenericType &&
            targetMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var method = typeof(RedisCachingProxy<TImplementation>)
                .GetMethod(nameof(InvokeAsyncWithResult), BindingFlags.NonPublic | BindingFlags.Instance)?
                .MakeGenericMethod(targetMethod.ReturnType.GetGenericArguments()[0]);

            return method?.Invoke(this, [targetMethod, args, GenerateCacheKey(targetMethod, args)]);
        }

        return InvokeSync(targetMethod, args, GenerateCacheKey(targetMethod, args));
    }

    private object? InvokeSync(MethodInfo method, object?[]? args, string cacheKey)
    {
        var cached = _redis.StringGet(cacheKey);
        if (cached.HasValue)
        {
            return JsonSerializer.Deserialize(cached.ToString(), method.ReturnType);
        }

        var result = method.Invoke(_decorated, args);
        _redis.StringSet(cacheKey, JsonSerializer.Serialize(result), _expiration);
        return result;
    }

    private async Task InvokeAsyncWithoutResult(MethodInfo method, object?[]? args)
    {
        var task = (Task)method.Invoke(_decorated, args)!;
        await task.ConfigureAwait(false);
    }

    private async Task<TResult> InvokeAsyncWithResult<TResult>(MethodInfo method, object?[]? args, string cacheKey)
    {
        var cached = await _redis.StringGetAsync(cacheKey).ConfigureAwait(false);
        if (cached.HasValue)
        {
            return JsonSerializer.Deserialize<TResult>(cached.ToString())!;
        }

        var task = (Task<TResult>)method.Invoke(_decorated, args)!;
        var result = await task.ConfigureAwait(false);
        await _redis.StringSetAsync(cacheKey, JsonSerializer.Serialize(result), _expiration)
            .ConfigureAwait(false);

        return result;
    }

    private static string GenerateCacheKey(MethodInfo method, object?[]? args)
    {
        if (args?.Length == 1 && args[0] is LambdaExpression expression)
        {
            var exprKey = expression.GetKeyExpression();
            return $"repo:{typeof(TImplementation).Name}:{method.Name}:expr:{exprKey}".ToLowerInvariant();
        }

        var argString = string.Join("_",
            (args ?? Array.Empty<object?>())
                .Where(static a => a is not null)
                .Select(static a => a!.ToString() ?? string.Empty));

        return $"repo:{typeof(TImplementation).Name}:{method.Name}:{argString}".ToLowerInvariant();
    }
}
