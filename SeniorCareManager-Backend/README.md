# SeniorCareManager-Backend

API REST do SeniorCare — ASP.NET Core 8, Entity Framework Core sobre
PostgreSQL. Serve os dois front-ends (`SeniorCareManager-Frontend`,
`SeniorStockManager-Frontend`) através de um único contrato HTTP versionado
(`/api/v1`).

Cobre autenticação por sessão (cookie + MFA), controle de acesso
(papéis/grupos de permissão/exceções/políticas), auditoria e os catálogos e
CRUDs assistenciais/de estoque existentes. O escopo funcional completo do
produto está em [`../docs/escopo-do-projeto.md`](../docs/escopo-do-projeto.md).

## Estrutura

- `SeniorCareManager.WebAPI` — a API em si (controllers, serviços, entidades,
  migrações). [`CONFIGURATION.md`](SeniorCareManager.WebAPI/CONFIGURATION.md)
  documenta toda variável de ambiente/configuração exigida por ambiente.
- `SeniorCareManager.UnitTests` — testes unitários (regras de negócio,
  validação, sem dependência externa).
- `SeniorCareManager.IntegrationTests` — testes de ponta a ponta contra um
  PostgreSQL efêmero real (Testcontainers), incluindo migração desde banco
  vazio e sobre dado pré-existente da versão anterior.

## Rodando localmente

Local, fora de container (precisa de um Postgres — veja
[`../infra/docker-test/README.md`](../infra/docker-test/README.md) para subir
um rápido):

```bash
cd SeniorCareManager.WebAPI
dotnet run
```

Ou a stack completa (API + os dois front-ends + Postgres) via Docker:

```bash
cd ../infra/docker-test && docker compose up -d --build
```

## Testes

```bash
dotnet test SeniorCareManager.WebAPI.sln --configuration Release
```

Roda unitários + integração (sobe um PostgreSQL real via Testcontainers —
precisa de Docker disponível). Já wired no CI (`.github/workflows/ci.yml`),
com resultados e cobertura publicados como artefato do job.

## Migrações

```bash
cd SeniorCareManager.WebAPI
dotnet tool restore   # instala o dotnet-ef pinado no manifesto local
dotnet ef migrations add <Nome>
dotnet ef database update
```

Em produção, a API aplica migração automaticamente no boot
(`Program.cs`) — não há passo manual de migração no deploy. O SQL das
migrações pendentes é gerado em CI e pré-validado (dry-run revertido) contra o
banco real antes de cada deploy; veja
[`../infra/deploy/BOOTSTRAP.md`](../infra/deploy/BOOTSTRAP.md).

## Documentação relacionada

- [Configuração e variáveis de ambiente](SeniorCareManager.WebAPI/CONFIGURATION.md)
- [Bootstrap da instituição e do administrador inicial](../infra/deploy/BOOTSTRAP.md)
- [Arquitetura de CI/CD](../docs/infra/ci-cd-arquitetura.md)
