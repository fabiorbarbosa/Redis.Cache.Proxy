# Redis.Cache.Proxy.Extensions

Extensões para simplificar o uso de `DispatchProxy` e Redis em projetos que desejam decorar repositórios com cache transparente. O pacote expõe atributos e métodos de DI para registrar o `IConnectionMultiplexer`, aplicar o proxy e controlar a expiração via `RedisCachedAttribute`.

## Instalação

```bash
dotnet add package Redis.Cache.Proxy.Extensions
```

## Uso rápido

1. **Registrar Redis** (por connection string ou com uma factory):

```csharp
services.AddRedisCacheProxy("localhost:6379");
```

2. **Marcar o repositório** com o atributo opcional para definir a expiração default:

```csharp
[RedisCached(expirationInMinutes: 5)]
public class CustomerRepository : ICustomerRepository
{
    // ...
}
```

3. **Aplicar o proxy** na DI para expor a interface decorada:

```csharp
services.AddRedisCachedRepository<ICustomerRepository, CustomerRepository>();
```

O proxy intercepta métodos síncronos e `Task`/`Task<T>` gerando chaves determinísticas a partir dos argumentos ou `Expression<Func<T,bool>>`.

## Recursos

- Registro de `IConnectionMultiplexer` pronto para produção.
- Atributo `RedisCachedAttribute` com tempo de expiração configurável.
- Geração de chaves compatível com queries por expressão (`Contains`, `BinaryExpression`, etc.).
- Preparado para multi-target (`netstandard2.0`, `net6.0`, `net7.0`, `net8.0`) e publicação no NuGet (README, ícone e símbolos inclusos).
