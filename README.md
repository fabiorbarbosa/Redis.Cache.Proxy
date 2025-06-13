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

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) (for Redis)
- Redis server running locally (via Docker) or remotely

### Running Redis with Docker

You can quickly start a Redis instance using Docker:

```sh
docker run --name redis-cache -p 6379:6379 -d redis
```

This will make Redis available at `localhost:6379`.

### Running the Application Locally

1. **Restore dependencies**
   ```sh
   dotnet restore Redis.Cache.Proxy.sln
   ```

2. **Build the solution**
   ```sh
   dotnet build Redis.Cache.Proxy.sln
   ```

3. **Run the API**
   ```sh
   dotnet run --project src/Redis.Cache.Proxy.Api/Redis.Cache.Proxy.Api.csproj
   ```

## API Documentation

The project supports three documentation UIs:

- **Swagger UI:**  
  [http://localhost:5000/docs](http://localhost:5000/docs)

- **ReDoc:**  
  [http://localhost:5000/api-doc](http://localhost:5000/api-doc)

- **Scalar:**  
  [http://localhost:5000/scalar](http://localhost:5000/scalar)

> All documentation endpoints are available when running in Development mode.

## Example Endpoints

- `GET /cities/{city}`  
  Returns clients filtered by city.
- `GET /cities`  
  Returns all clients.

## Main Technologies

- ASP.NET Core 9.0
- Redis (via StackExchange.Redis)
- Swagger (Swashbuckle)
- ReDoc
- Scalar
- Dependency Injection (Microsoft.Extensions.DependencyInjection)

## Dockerfile Example (for Redis only)

If you want to run Redis using Docker, you don't need a custom Dockerfile. Use the official Redis image as shown above.

## Contributing

1. Fork this repository
2. Create your branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -am 'feat: your feature'`)
4. Push to the branch (`git push origin feature/your-feature`)
5. Open a Pull Request

---

> This project is for educational purposes and demonstrates a modular architecture with