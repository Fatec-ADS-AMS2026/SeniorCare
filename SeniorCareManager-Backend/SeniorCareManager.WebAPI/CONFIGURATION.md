# Configuração — SeniorCareManager.WebAPI

A API lê configuração da forma padrão do ASP.NET Core: `appsettings.json` →
`appsettings.{ASPNETCORE_ENVIRONMENT}.json` → variáveis de ambiente (a última
camada sempre vence). Segredos e valores por ambiente **nunca** ficam nos
`appsettings.*.json` versionados — só variáveis de ambiente.

## Ambientes

| `ASPNETCORE_ENVIRONMENT` | Arquivo | Uso |
|---|---|---|
| `Development` (default se não definido) | `appsettings.Development.json` | Rodar localmente fora de container. Tem uma senha de banco local (`postdba`) — só funciona contra o Postgres do `infra/docker-test/`, nunca aponta pra um ambiente real. |
| `Test` | `appsettings.Test.json` | Usado pelos testes de integração (`PostgresWebApplicationFactory`). Não tem `ConnectionStrings` — a fábrica de testes substitui o `DbContext` por código, apontando pro contêiner Postgres efêmero do Testcontainers. |
| `Production` | `appsettings.Production.json` | Deploy real (`infra/deploy/`). Deliberadamente **sem** `ConnectionStrings` nem CORS — essas variáveis têm que vir de fora (ver tabela abaixo); se faltarem, o processo falha no boot (ver `Program.cs`). |

## Variáveis obrigatórias em produção

| Variável | Formato | Onde é setada |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | string de conexão Npgsql, ex.: `Host=postgres;Port=5432;Database=db_seniorcare;Username=postgres;Password=<senha>;` | `infra/deploy/docker-compose.yml`, montada a partir de `clients/<nome>/.env` (ver `infra/deploy/clients/exemplo/.env.example`) |
| `CORS_ALLOWED_ORIGINS` | lista separada por vírgula, ex.: `https://care.exemplo.com.br,https://estoque.exemplo.com.br` | idem — sem essa variável, o CORS cai no default de desenvolvimento (`localhost:3000`/`:3001`/`:5173`), que não funciona em produção |

O nome com `__` (duplo underscore) é a convenção do ASP.NET Core para mapear
uma variável de ambiente pra uma chave aninhada (`ConnectionStrings:DefaultConnection`).

## Validação de startup

O processo verifica, antes de subir, que `ConnectionStrings:DefaultConnection`
está presente e não vazia — se faltar, encerra com uma mensagem de erro que
identifica a chave ausente, sem nunca ecoar o valor configurado (mesmo que
inválido). Ver `Program.cs`.

## O que ainda falta documentar aqui

Validação de chaves de autenticação, dados da instituição e credenciais de
bootstrap administrativo ainda não se aplica — essas configurações entram
junto com a capability `platform-authentication` (specs/tasks §4 em diante),
que ainda não existe no código. Esta seção será expandida quando esse
trabalho for feito.
