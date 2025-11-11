# Redis.Cache.Proxy

<p align="center">
  <img src="assets/redis-cache-proxy.svg" alt="Redis Cache Proxy logo" width="200" />
</p>

Modular .NET 9.0 sample showing how to wrap repositories behind a Redis-powered `DispatchProxy`, with a minimal API front-end, clean layering, and the `Redis.Cache.Proxy.Extensions` package ready for NuGet distribution.

## Architecture Overview

- **src/Redis.Cache.Proxy.Api/**  
  Minimal API exposing the `/cities` endpoints and wiring the entire stack.
- **src/Redis.Cache.Proxy.Infra/**  
  Dependency injection configuration (`DependencyInjectionConfig`) that registers Redis, contexts, services, and cached repositories.
- **src/Redis.Cache.Proxy.Application/**  
  Application services (`ICustomerService`, `CustomerService`) and DTOs consumed by the API.
- **src/Redis.Cache.Proxy.Data/**  
  Entities, fake in-memory context (`FakeDbSet`) and repositories marked with `[RedisCached]`.
- **src/Redis.Cache.Proxy.Extensions/**  
  Extractable library (NuGet-ready) containing attributes, factories, and helpers for the cache proxy.

## Redis.Cache.Proxy.Extensions

### Key Features
- `[RedisCached]` attribute to define custom TTL per repository.
- `RedisCacheProxyServiceCollectionExtensions` helpers to register `IConnectionMultiplexer` and decorate repositories via `AddRedisCachedRepository<TInterface, TImplementation>()`.
- `RedisCachingProxy` built on `DispatchProxy`, serializing results with `System.Text.Json`.
- Deterministic cache key generation for LINQ expressions (`ExpressionExtensions` + `KeyExpressionVisitor`).
- Multi-target build (`netstandard2.0`, `net6.0`, `net7.0`, `net8.0`) plus NuGet assets (`logo.png`, `redis-cache-proxy.svg`, README).

### Install via NuGet
Once published:
```sh
dotnet add package Redis.Cache.Proxy.Extensions
```

### Typical Usage
```csharp
services.AddRedisCacheProxy("localhost:6379");
services.AddRedisCachedRepository<ICustomerRepository, CustomerRepository>(
    defaultExpiration: TimeSpan.FromMinutes(30));
```
Add the attribute on the concrete repository:
```csharp
[RedisCached(expirationInMinutes: 5)]
internal class CustomerRepository : ICustomerRepository { ... }
```

## Running locally

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) to spin up Redis

### Start Redis via Docker
```sh
docker run --name redis-cache -p 6379:6379 -d redis
```

### Steps to run the API
```sh
dotnet restore Redis.Cache.Proxy.sln
dotnet build Redis.Cache.Proxy.sln
dotnet run --project src/Redis.Cache.Proxy.Api/Redis.Cache.Proxy.Api.csproj
```

## API Documentation
- **Swagger UI:** http://localhost:5000/docs  
- **ReDoc:** http://localhost:5000/api-doc  
- **Scalar:** http://localhost:5000/scalar  

> Available when the app runs under the `Development` environment (see `Program.cs`).

### Sample Endpoints
- `GET /cities` → returns all customers and total count.
- `GET /cities/{city}` → filters customers by city (case-insensitive).

## Technologies and tools
- ASP.NET Core 9.0 (Minimal API + `AddProblemDetails` + `AddOutputCache`)
- StackExchange.Redis 2.7.33
- Swagger UI / ReDoc / Scalar for docs
- Docker + official `redis` image
- GitHub Actions to pack and publish the Extensions package

## NuGet Publishing
- Workflow: `.github/workflows/publish-extensions.yml`
- Trigger: push tags `extensions-v*` (e.g., `extensions-v1.0.2`) or run manually.
- Output: `dotnet pack` of `src/Redis.Cache.Proxy.Extensions/Redis.Cache.Proxy.Extensions.csproj` (Release) followed by `dotnet nuget push` to nuget.org.
- Required secret: `NUGET_API_KEY` with push permission.
- Before tagging: bump `<VersionPrefix>` in the `.csproj`, validate with `dotnet pack`, ensure assets (`README.md`, `logo.png`, `redis-cache-proxy.svg`) are up to date, then create/push the tag.

## Contributing
1. Fork this repository.
2. Create a branch (`git checkout -b feature/my-feature`).
3. Implement changes, add tests when relevant, and run `dotnet build`.
4. Commit (`git commit -am 'feat: my feature'`) and push (`git push origin feature/my-feature`).
5. Open a Pull Request describing API and/or NuGet package impacts.

---

> This repository is an educational guide on extracting a reusable Redis DispatchProxy extension while keeping a clean layered architecture.
