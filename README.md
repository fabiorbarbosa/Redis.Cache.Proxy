# Simplify.Cache.Proxy

<p align="center">
  <img src="assets/redis-cache-proxy.svg" alt="Redis Cache Proxy logo" width="200" />
</p>

Biblioteca reutilizável que demonstra como encapsular repositórios atrás de um `DispatchProxy` com Redis. O repositório agora mantém apenas dois projetos:

- `src/Simplify.Cache.Proxy/` → pacote multi-target (namespace `Simplify.Cache.Proxy`) pronto para o NuGet.
- `tests/Simplify.Cache.Proxy.Tests/` → suíte de testes em xUnit (namespace `Simplify.Cache.Proxy.Tests`) que valida o comportamento público da extensão.

## Simplify.Cache.Proxy

### Principais recursos
- `[RedisCached]` para definir TTL por repositório (mínimo de 30 min).
- `AddRedisCacheProxy` registra `IConnectionMultiplexer` via connection string ou factory customizada.
- `AddRedisCachedRepository<TInterface, TImplementation>()` decora a implementação concreta com um `DispatchProxy` que lê/escreve em Redis.
- `RedisCachingProxy` serializa com `System.Text.Json` e gera chaves determinísticas para expressões LINQ (`ExpressionExtensions` + `KeyExpressionVisitor`).
- Multi-target (`netstandard2.0`, `net6.0`, `net7.0`, `net8.0`) incluindo README e ícone no pacote.

### Instalação (NuGet)
```sh
dotnet add package Simplify.Cache.Proxy.Extensions
```

### Uso típico
```csharp
services.AddRedisCacheProxy("localhost:6379");
services.AddRedisCachedRepository<ICustomerRepository, CustomerRepository>(
    defaultExpiration: TimeSpan.FromMinutes(30));

[RedisCached(expirationInMinutes: 5)]
internal sealed class CustomerRepository : ICustomerRepository
{
    // ...
}
```

## Fluxo de desenvolvimento

### Requisitos
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Restaurar, compilar e testar
```sh
dotnet restore Simplify.Cache.Proxy.sln
dotnet build Simplify.Cache.Proxy.sln -c Release
dotnet test Simplify.Cache.Proxy.sln
```

### Empacotar a extensão
```sh
dotnet pack src/Simplify.Cache.Proxy/Simplify.Cache.Proxy.csproj \
  -c Release -o ./artifacts
```

## Estratégia de testes
Os testes vivem em `tests/Simplify.Cache.Proxy.Tests` (xUnit + FluentAssertions + NSubstitute) e cobrem:
- Geração de chaves para expressões diferentes.
- Cache miss/hit para métodos async/sync.
- Aplicação correta das políticas de expiração (atributo vs. `defaultExpiration`).
- Registro via `IServiceCollection` e injeção de `IConnectionMultiplexer`.

Execute `dotnet test` antes de qualquer PR e ao ajustar os cenários listados acima.

## Publicação no NuGet
- Workflow (`.github/workflows/publish-extensions.yml`) continua responsável por empacotar e publicar.
- Dispare com tags `extensions-v*` (ex.: `extensions-v1.0.7`) ou manualmente via GitHub Actions.
- Confirme `dotnet pack` localmente, atualize `<VersionPrefix>` e mantenha README/ícones sincronizados antes de publicar.

## Contribuindo
1. Fork/branch (`git checkout -b feature/nome`).
2. Implemente a mudança, acrescente testes pertinentes.
3. Rode `dotnet build` + `dotnet test`.
4. Commit (`git commit -am 'feat: descrição'`) e push.
5. Abra o PR descrevendo impactos no pacote.

---

> O objetivo é manter uma extensão enxuta, multi-target e bem testada para quem deseja reutilizar proxies de cache com Redis em seus próprios repositórios.
