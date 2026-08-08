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

## Bootstrap da instituição e do administrador inicial

| Variável | Formato | Obrigatório quando |
|---|---|---|
| `Bootstrap__InstitutionName` | nome da ILPI, ex.: `ILPI Exemplo` | nenhuma instituição existir ainda no banco |
| `Bootstrap__AdminEmail` | e-mail da primeira conta administrativa | idem |
| `Bootstrap__AdminDisplayName` | nome de exibição da primeira conta administrativa | idem |

As três variáveis só fazem sentido juntas — informar só uma ou duas é erro de
configuração e o processo falha no boot (ver "Validação de startup" abaixo).
No primeiro boot sem nenhuma instituição, se as três estiverem presentes, o
processo cria a instituição e uma conta administrativa `PROVISIONED` **sem
senha conhecida** e imprime no console, uma única vez, o link/token de
ativação — não é persistido em nenhum lugar além do hash, então precisa ser
capturado nesse momento. Reinícios seguintes (instituição já existente) são
no-op: nada é recriado nem redefinido silenciosamente, mesmo que as
variáveis continuem definidas.

## Validação de startup

O processo verifica, antes de subir, que `ConnectionStrings:DefaultConnection`
está presente e não vazia, e que as três variáveis de bootstrap acima foram
informadas todas juntas ou nenhuma — se algo faltar, encerra com uma mensagem
de erro que identifica a(s) chave(s) ausente(s)/incompleta(s), sem nunca
ecoar nenhum valor configurado. Ver `Program.cs`.

## O que ainda falta documentar aqui

Validação de chaves de sessão/autenticação (JWT/cookie) e dos parâmetros de
MFA, bloqueio e duração de sessão ainda não se aplica — essas configurações
entram junto com as capabilities de sessão e RBAC (specs/tasks §5 em diante),
que ainda não existem no código. Esta seção será expandida quando esse
trabalho for feito.
