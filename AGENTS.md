# AGENTS.md — Redis.Cache.Proxy

## Missão do Agente
Atuar como engenheiro sênior .NET responsável por evoluir um gateway minimal API que demonstra como encapsular repositórios atrás de um proxy com Redis. O agente deve preservar a arquitetura em camadas (`Api → Infra → Application → Data → Extensions`), manter a biblioteca `Redis.Cache.Proxy.Extensions` pronta para publicação no NuGet e garantir que a API continue simples, performática e bem documentada.

## Stack detectado
- **Runtime principal:** .NET 9.0 (`Redis.Cache.Proxy.Api`, `Application`, `Data`, `Infra`) com C# 12, nullability e `ImplicitUsings`.
- **Biblioteca compartilhada:** `Redis.Cache.Proxy.Extensions` multi-target (`netstandard2.0`, `net6.0`, `net7.0`, `net8.0`) para distribuição NuGet.
- **Framework web:** ASP.NET Core Minimal API com `AddProblemDetails`, `AddOutputCache`, `Microsoft.AspNetCore.OpenApi 9.0.6`, `Scalar.AspNetCore 2.4.15`, Swagger UI e ReDoc (9.0.1).
- **Cache:** Redis via `StackExchange.Redis 2.7.33`, com proxies baseados em `DispatchProxy`.
- **Solução:** `Redis.Cache.Proxy.sln`, organizada em projetos por camada sob `src/`.
- **Infra externa:** Redis container Dockerfile (base `redis:alpine`) + instrução `docker run --name redis-cache -p 6379:6379 -d redis`.

## Baseline cross-stack
- **Estrutura de pastas:** cada camada em `src/Redis.Cache.Proxy.<Layer>/`. A solução Visual Studio agrupa-as em pastas lógicas (`1 - API`, `2 - Application`, etc.).
- **Naming:** namespaces e classes seguem `Redis.Cache.Proxy.<Layer>.<Feature>`. Serviços e repositórios usam prefixos explícitos (`ICustomerService`, `CustomerRepository`). Classes compartilhadas preferem `internal`; acesso cruzado é liberado via `InternalsVisibleTo`.
- **Abordagem arquitetural:** Clean layering. API registra tudo via `Redis.Cache.Proxy.Infra` (`DependencyInjectionConfig.AddDependencies`). Application expõe interfaces, Data provê repositórios concretos, Infra injeta dependências e Extensions fornece a infraestrutura de cache.
- **Documentação / DX:** Endpoints HTTP expostos por `Program.cs` usam `MapGet`. Documentação interativa (Swagger UI, ReDoc, Scalar) habilitada apenas em `Development`.
- **Padrões de código:** Namespaces em escopo de arquivo, `internal sealed` sempre que possível, records não utilizados. Injeção por construtores primários (C# 12). LINQ + `async/await` para operações.
- **Dados fake:** `CacheDbContext` fornece um `FakeDbSet<T>` para simular EF Core; repositórios filtram via expressões compatíveis com geração de cache key determinística.

## Camadas e convenções específicas

### API (`src/Redis.Cache.Proxy.Api`)
- `Program.cs` usa top-level statements. Ordem recomendada: serviços → `builder.Build()` → `if Development` middlewares → endpoints.
- Registra `builder.Services.AddDependencies();` (Infra). Não adicionar DI direto neste projeto.
- Endpoints:
  - `GET /cities` retorna `Total` + `Cities`.
  - `GET /cities/{city}` filtra (case-insensitive) via `ICustomerService.GetByCityAsync`.
- Cache HTTP: `AddOutputCache` com policy base de 10 minutos. Ajustes devem seguir política central.
- Documentação: `app.MapOpenApi()`, `UseSwaggerUI`, `UseReDoc`, `MapScalarApiReference`.

### Infra (`src/Redis.Cache.Proxy.Infra`)
- Único ponto para orquestrar serviços. `DependencyInjectionConfig.AddDependencies` deve continuar idempotente.
- Registra Redis via `services.AddRedisCacheProxy("localhost:6379");`. Para ambientes reais, sobrescreva com config/env var e mantenha string fora do controle de versão.
- Responsável por registrar contextos (`CacheDbContext`), serviços (`ICustomerService`) e decorar repositórios via `AddRedisCachedRepository`.

### Application (`src/Redis.Cache.Proxy.Application`)
- Expõe contratos (`ICustomerService`) e DTOs (`CustomerModel`). Classes de implementação são `internal`.
- Conversão Entity→Model feita manualmente em `CustomerService`; mantenha mapeamentos explícitos (evitar AutoMapper).
- Métodos retornam `Task<IEnumerable<...>>`; preferir `IAsyncEnumerable` apenas se necessário (lembre-se da compatibilidade com proxy e cache).

### Data (`src/Redis.Cache.Proxy.Data`)
- Fornece entidades (`Entities/Customer`), contexto fake (`Contexts`) e repositórios (`Repositories`).
- `FakeDbSet` implementa `IQueryable` + `IAsyncEnumerable` para simular EF Core. Evitar APIs específicas de EF; use LINQ padronizado.
- `CustomerRepository` é decorado com `[RedisCached(expirationInMinutes: 1)]`. Novos repositórios devem definir o atributo com TTL adequado para orientar o proxy.
- Métodos expostos aceitam `Expression<Func<TEntity,bool>>` para permitir geração de cache key via `ExpressionExtensions`.

### Redis Cache Proxy Extensions (`src/Redis.Cache.Proxy.Extensions`)
- Biblioteca reutilizável, pensada para NuGet (`PackageId`, metadata, README, ícone em `assets/logo.png`).
- Conteúdo:
  - `RedisCachedAttribute` define TTL padrão (mínimo 30 min se valor <= 0).
  - `RedisCacheProxyServiceCollectionExtensions` oferece:
    - `AddRedisCacheProxy` (string ou factory) → registra `IConnectionMultiplexer`.
    - `AddRedisCachedRepository<TInterface, TImplementation>` → cria `DispatchProxy` com cache.
  - `RedisCachingProxy`, `RedisProxyFactory` implementam interceptação. `ExpressionExtensions` + `KeyExpressionVisitor` transformam `Expression` em chave determinística (`repo:{Repo}:{Method}:expr:{...}`).
- Ao evoluir a lib:
  - Manter compatibilidade com todos os TFM listados.
  - Atualizar pacote via `dotnet pack` e preservar README/icon no `.csproj`.

## Ferramentas e scripts
- Restaurar dependências: `dotnet restore Redis.Cache.Proxy.sln`.
- Build completo: `dotnet build Redis.Cache.Proxy.sln -c Release`.
- Executar API: `dotnet run --project src/Redis.Cache.Proxy.Api/Redis.Cache.Proxy.Api.csproj`.
- Subir Redis local (Docker): `docker run --name redis-cache -p 6379:6379 -d redis`.
- Empacotar Extensions para NuGet: `dotnet pack src/Redis.Cache.Proxy.Extensions/Redis.Cache.Proxy.Extensions.csproj -c Release -o ./artifacts`.
- Opcional: `dotnet format` para manter estilo; ainda não configurado, mas recomendado antes do PR.

## Checklist de PR
1. `dotnet restore` + `dotnet build` sem warnings novos em todos os projetos.
2. Executar a API contra um Redis real (docker) e validar `GET /cities` e `GET /cities/{city}` (confirmar cache hit via Redis se possível).
3. Se mexer na `Redis.Cache.Proxy.Extensions`, rodar `dotnet pack` para garantir que multi-target continua compilando e que README/icon estão no pacote.
4. Verificar documentação interativa (Swagger / ReDoc / Scalar) ao alterar contratos.
5. Atualizar README/AGENTS se alterar fluxo de build, comandos ou estrutura.
6. Código novo respeita padrões de namespace, acessibilidade (`internal` por padrão) e atributos `[RedisCached]` quando usar proxies.

## Regras de segurança e performance
- Nunca versionar strings de conexão reais; use variáveis de ambiente ou `dotnet user-secrets` e mantenha `AddRedisCacheProxy` parametrizado.
- TTLs devem ser positivos; o atributo já aplica fallback de 30 min, mas valide entradas externas antes de criar proxies customizados.
- Evitar operações bloqueantes dentro de repositórios/serviços; use `async` e LINQ em memória apenas em camadas fake.
- Serialização do cache usa `System.Text.Json`; assegurar que objetos retornados sejam serializáveis e não contenham referências cíclicas.
- Ao mexer no `RedisCachingProxy`, preserve geração de chaves determinísticas (`GenerateCacheKey`) para não invalidar cache existente.
- Dockerfile expõe Redis em 6379; para produção, reforçar autenticação do Redis e considerar TLS (fora do escopo atual, mas mencionar em PRs que tocarem infra).

## Estilo de comunicação do agente
Técnico e direto, com foco em contexto arquitetural: explique decisões, cite arquivos (`caminho:linha`) quando necessário e proponha próximos passos de forma objetiva. Documente lacunas (ex.: ausência de testes automatizados) e sugira mitigação sempre que tocar em segurança ou performance.
