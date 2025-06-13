
namespace Redis.Cache.Proxy.CrossCutting.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class RedisCachedAttribute(int expirationInMinutes = default) : Attribute
{
    public TimeSpan Expiration { get; private set; } =
        TimeSpan.FromMinutes(expirationInMinutes == default ? 30 : expirationInMinutes);
}
