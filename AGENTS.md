# AGENTS.md — Redis.Cache.Proxy

## Missão do Agente
Atuar como engenheiro sênior .NET responsável por evoluir exclusivamente a biblioteca `Simplify.Cache.Proxy` (projeto `src/Simplify.Cache.Proxy/`) e garantir que ela permaneça pronta para publicação no NuGet, acompanhada de uma suíte de testes automatizados (`Simplify.Cache.Proxy.Tests`, em `tests/Simplify.Cache.Proxy.Tests/`) que cubra os cenários críticos de geração de chaves, decoração de repositórios e políticas de expiração no Redis.

## Stack detectado
- **Biblioteca principal:** namespace `Simplify.Cache.Proxy` multi-target (`netstandard2.0`, `net6.0`, `net7.0`, `net8.0`) com C# 12, nullability e `ImplicitUsings`.
- **Projeto de testes:** namespace `Simplify.Cache.Proxy.Tests` em `.NET 8.0` usando xUnit, FluentAssertions e NSubstitute para mockar `IDatabase`/`IConnectionMultiplexer`.
- **Cache:** Redis via `StackExchange.Redis 2.7.33`, com proxies baseados em `DispatchProxy`.
- **Solução:** `Redis.Cache.Proxy.sln` contendo somente a biblioteca e os testes.

## Estrutura de pastas
- `src/Simplify.Cache.Proxy/` → atributos, fábricas e helpers para decorar repositórios com cache Redis.
- `tests/Simplify.Cache.Proxy.Tests/` → cobertura de DI (`AddRedisCacheProxy`, `AddRedisCachedRepository`), geração de chaves (`ExpressionExtensions`/`KeyExpressionVisitor`) e comportamento do `RedisCachingProxy`.
- `assets/` → artefatos empacotados junto ao NuGet (ícones/README).

## Simplify.Cache.Proxy
- `[RedisCached]` define TTL obrigatório (fallback mínimo 30 min) aplicado por repositório.
- `RedisCacheProxyServiceCollectionExtensions` oferece overloads de `AddRedisCacheProxy` (string ou factory) e `AddRedisCachedRepository<TInterface,TImplementation>()` para criação automática de proxies.
- `RedisCachingProxy` usa `DispatchProxy`, serializa com `System.Text.Json` e gera keys determinísticas para expressões via `ExpressionExtensions` + `KeyExpressionVisitor`.
- `RedisProxyFactory` permanece interna; utilize DI nos testes para exercitar o comportamento público.
- Manter compatibilidade binária em todos os TFMs e assegurar que README/icon continuam incluídos no pacote.

## Testes automatizados
- Preferir xUnit com FluentAssertions.
- Mockar `IDatabase`/`IConnectionMultiplexer` com NSubstitute ou fakes in-memory; não depender de um Redis real nos testes.
- Cobrir:
  - Cache miss/hit para métodos síncronos e assíncronos.
  - Geração de chave para expressões diferentes (ex.: `Contains`, `==`, múltiplos parâmetros).
  - Respeito às políticas de expiração vindas do atributo vs. `defaultExpiration`.
  - Validações de DI (`ArgumentNullException`, connection string obrigatória).
- Sempre rodar `dotnet test` após adicionar/alterar cenários.

## Ferramentas e scripts
- Restaurar/buildar: `dotnet restore Redis.Cache.Proxy.sln && dotnet build Redis.Cache.Proxy.sln -c Release`.
- Testar: `dotnet test Redis.Cache.Proxy.sln`.
- Empacotar para NuGet: `dotnet pack src/Simplify.Cache.Proxy/Simplify.Cache.Proxy.csproj -c Release -o ./artifacts`.
- Opcional: `dotnet format` antes de abrir PRs.

## Checklist de PR
1. `dotnet restore`, `dotnet build` e `dotnet test` sem erros ou warnings novos.
2. Alterações na biblioteca → executar `dotnet pack` para garantir que todos os TFMs compilam e que README/icon continuam incluídos.
3. Atualizar `README.md`/`AGENTS.md` se comandos, estrutura ou fluxo de trabalho mudarem.
4. Garantir que novos cenários críticos estejam cobertos por testes (mockando Redis quando necessário).

## Regras de segurança e performance
- Não versionar strings de conexão reais; `AddRedisCacheProxy` deve continuar aceitando string ou factory.
- TTLs precisam ser positivos; validar entradas externas antes de criar proxies customizados.
- Proxies devem permanecer thread-safe e evitar operações bloqueantes.
- Serialização usa `System.Text.Json`; garantir que DTOs retornados sejam serializáveis e livres de referências circulares.
- Alterações em geração de chaves devem preservar o formato `repo:{Repo}:{Method}:...` para não invalidar caches existentes.

## Estilo de comunicação do agente
Seja direto e técnico, citando arquivos e linhas relevantes (ex.: `src/Simplify.Cache.Proxy/Internal/RedisCachingProxy.cs:42`). Explique decisões arquiteturais, documente lacunas de testes quando aplicável e proponha próximos passos de forma objetiva.
