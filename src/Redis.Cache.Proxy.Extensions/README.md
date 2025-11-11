# Redis.Cache.Proxy.Extensions

<p align="center">
  <img src="redis-cache-proxy.svg" alt="Redis Cache Proxy logo" width="160" />
</p>

Helpers that wrap repositories behind a Redis-backed `DispatchProxy`, exposing attributes and DI extensions so you can enable transparent caching with a single registration call.

## Installation

```bash
dotnet add package Redis.Cache.Proxy.Extensions
```

## Quick start

1. **Register Redis** (connection string or factory):

```csharp
services.AddRedisCacheProxy("localhost:6379");
// or services.AddRedisCacheProxy(provider => ConnectionMultiplexer.Connect(...));
```

2. **Annotate the repository** (optional TTL override):

```csharp
[RedisCached(expirationInMinutes: 5)]
internal class CustomerRepository : ICustomerRepository
{
    // ...
}
```

3. **Expose the interface through the proxy**:

```csharp
services.AddRedisCachedRepository<ICustomerRepository, CustomerRepository>(
    defaultExpiration: TimeSpan.FromMinutes(30));
```

The proxy intercepts sync and async methods (`Task` / `Task<T>`), generating deterministic cache keys from method arguments or `Expression<Func<T,bool>>` filters.

## Features

- Production-ready `IConnectionMultiplexer` registration helpers.
- `RedisCachedAttribute` with configurable expiration and sane defaults.
- Deterministic cache key generation for expression-based queries (`Contains`, `BinaryExpression`, etc.).
- Multi-target (`netstandard2.0`, `net6.0`, `net7.0`, `net8.0`) with NuGet assets included (`README`, `logo.png`, `redis-cache-proxy.svg`, symbol packages).
