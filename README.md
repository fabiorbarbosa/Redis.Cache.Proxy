# Redis.Cache.Proxy

A modular .NET 9.0 project that provides a cache proxy using Redis, organized into multiple layers for maintainability and scalability.

## Project Structure

- **src/Redis.Cache.Proxy.Api/**  
  Main API project exposing HTTP endpoints for cache and data interaction.
- **src/Redis.Cache.Proxy.CrossCutting/**  
  Shared utilities and cross-cutting concerns.
- **src/Redis.Cache.Proxy.Data/**  
  Data access layer: entities, contexts, and repositories.
- **src/Redis.Cache.Proxy.Infra/**  
  Infrastructure and dependency injection configuration.
- **src/Redis.Cache.Proxy.Services/**  
  Domain services, business logic, and integrations.

## How to Run

1. **Prerequisites**
   - [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
   - Redis server running locally or remotely

2. **Restore dependencies**
   ```sh
   dotnet restore Redis.Cache.Proxy.sln
   ```

3. **Build the solution**
   ```sh
   dotnet build Redis.Cache.Proxy.sln
   ```

4. **Run the API**
   ```sh
   dotnet run --project src/Redis.Cache.Proxy.Api/Redis.Cache.Proxy.Api.csproj
   ```

5. **Access Swagger documentation**
   - After starting the API, go to: [http://localhost:5000](http://localhost:5000)  
     (or the configured port)
   - Interactive documentation will be available at the root via Swagger UI.

## Endpoints

- `GET /clientes/{cidade}`  
  Returns clients filtered by city.
- `GET /clientes`  
  Returns all clients.

## Main Technologies

- ASP.NET Core 9.0
- Redis (via StackExchange.Redis)
- Swagger (Swashbuckle)
- Dependency Injection (Microsoft.Extensions.DependencyInjection)

## Contributing

1. Fork this repository
2. Create your branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -am 'feat: your feature'`)
4. Push to the branch (`git push origin feature/your-feature`)
5. Open a Pull Request

---

> This project is for educational purposes and demonstrates a modular architecture