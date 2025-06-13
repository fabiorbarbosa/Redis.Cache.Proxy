using StackExchange.Redis;

using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Redis.Cache.Proxy.CrossCutting.Cache;

internal class RedisCachingProxy<T> : DispatchProxy
{
    private T _decorated = default!;
    private IDatabase _redis = default!;
    private TimeSpan? _expiration;
    private readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    public void Configure(T decorated, IDatabase redis, TimeSpan? expiration = null)
    {
        ArgumentNullException.ThrowIfNull(decorated);
        ArgumentNullException.ThrowIfNull(redis);

        if (expiration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(expiration), "Expiration must be non-negative.");
        }

        _decorated = decorated;
        _redis = redis;
        _expiration = expiration ?? DefaultExpiration;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);

        if (targetMethod.ReturnType == typeof(Task))
        {
            var method = typeof(RedisCachingProxy<T>)
                .GetMethod(nameof(InvokeAsyncWithoutResult), BindingFlags.NonPublic | BindingFlags.Instance);

            return method?.Invoke(this, [targetMethod, args!]);
        }
        else if (targetMethod.ReturnType.IsGenericType && targetMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var method = typeof(RedisCachingProxy<T>)
                .GetMethod(nameof(InvokeAsyncWithResult), BindingFlags.NonPublic | BindingFlags.Instance)?
                .MakeGenericMethod(targetMethod.ReturnType.GetGenericArguments().FirstOrDefault()!);

            return method?.Invoke(this, [targetMethod, args!, GenerateCacheKey(targetMethod, args)]);
        }

        return InvokeSync(targetMethod, args, GenerateCacheKey(targetMethod, args));
    }

    private object? InvokeSync(MethodInfo method, object?[]? args, string cacheKey)
    {
        var cached = _redis.StringGet(cacheKey);

        if (cached.HasValue)
        {
            return JsonSerializer.Deserialize(cached.ToString(), method.ReturnType)!;
        }

        var result = method.Invoke(_decorated, args);
        _redis.StringSet(cacheKey, JsonSerializer.Serialize(result), _expiration);

        return result;
    }

    private async Task InvokeAsyncWithoutResult<TResult>(MethodInfo method, object?[]? args)
    {
        var taskExec = (Task)method.Invoke(_decorated, args)!;
        await taskExec.ConfigureAwait(false);
    }

    private async Task<TResult> InvokeAsyncWithResult<TResult>(MethodInfo method,
        object?[]? args, string cacheKey)
    {
        var cached = await _redis.StringGetAsync(cacheKey);

        if (cached.HasValue)
        {
            return JsonSerializer.Deserialize<TResult>(cached.ToString())!;
        }

        var taskExec = (Task<TResult>)method.Invoke(_decorated, args!)!;
        var result = await taskExec.ConfigureAwait(false);
        var setOk = await _redis.StringSetAsync(cacheKey, JsonSerializer.Serialize(result), DefaultExpiration);
        Console.WriteLine($"[Cache SET] Key={cacheKey}, Success={setOk}");

        return result;
    }

    private static string GenerateCacheKey(MethodInfo method, object?[]? args)
    {
        if (args?.Length == 1 && args[0] is LambdaExpression expr)
        {
            var visitor = new ExpressionKeyVisitor();
            visitor.Visit(expr);

            var exprKey = visitor.GetKey();
            return $"repo:{typeof(T).Name}:{method.Name}:expr:{exprKey}";
        }

        var argString = string.Join("_", (args ?? []).Where(a => a is not null).Select(a => a?.ToString() ?? string.Empty));

        return $"repo:{typeof(T).Name}:{method.Name}:{argString}";
    }
}