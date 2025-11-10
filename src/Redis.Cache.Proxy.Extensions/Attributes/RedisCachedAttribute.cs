namespace Redis.Cache.Proxy.Extensions.Attributes;

/// <summary>
/// Marks a concrete repository so it can be wrapped by a Redis DispatchProxy with a custom expiration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class RedisCachedAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCachedAttribute"/> class.
    /// </summary>
    /// <param name="expirationInMinutes">Expiration in minutes (defaults to 30 when zero or negative).</param>
    public RedisCachedAttribute(int expirationInMinutes = 30)
    {
        if (expirationInMinutes <= 0)
        {
            expirationInMinutes = 30;
        }

        Expiration = TimeSpan.FromMinutes(expirationInMinutes);
    }

    /// <summary>
    /// Gets the expiration configured for the cached repository.
    /// </summary>
    public TimeSpan Expiration { get; }
}
