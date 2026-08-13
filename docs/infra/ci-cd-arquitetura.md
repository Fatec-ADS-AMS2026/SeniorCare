# Arquitetura de CI/CD — SeniorCare

> **Documento de referência / ADR** do modelo de entrega contínua do SeniorCare.
> Adaptado do modelo usado no QualitasSystem para a stack deste projeto (.NET 8 Web
> API + dois frontends React/Vite). Os artefatos de pipeline (`.github/workflows/`,
> `infra/deploy/`, `deploy.sh`) implementam o que está aqui.
>
> _Status: PROPOSTO — ainda sem servidor de produção definido._

## 1. Contexto e restrições

O SeniorCare é um monorepo com 3 unidades implantáveis (backend + 2 frontends), ainda
sem servidor de destino definido. As decisões abaixo assumem o mesmo modelo do
QualitasSystem — pull-based via GHCR — mas escaladas para um único ambiente por vez
(sem a estrutura de múltiplos clientes hospitalares que motivou aquele desenho; a pasta
`clients/<nome>` foi mantida porque não custa nada e permite crescer para mais de um
ambiente sem redesenho).

| Restrição | Valor | Impacto no desenho |
|---|---|---|
| Plataforma de CI | GitHub Actions (`github.com/Fatec-ADS-AMS2026/SeniorCare`, repo público) | CI + `ghcr.io` integrados; Code Scanning nativo é gratuito por ser público. |
| Runtime no servidor | Docker + Docker Compose | Mesmo padrão de `infra/docker-test/`. |
| Persistência | PostgreSQL 16 + bind mount | Stateful; migrations do EF Core rodam no boot da API (`Program.cs`); backup antes de deploy. |

## 2. Decisão central: `build once, deploy many`

A **imagem Docker versionada e imutável** é o artefato de entrega. O servidor de
produção **nunca compila** — apenas autentica no GHCR, puxa a imagem já construída e
sobe. Compilar (CI) e entregar (CD) ficam desacoplados.

## 3. Unidades implantáveis

| Unidade | Stack | Build | Imagem (GHCR) |
|---|---|---|---|
| `api` | ASP.NET Core 8 / EF Core + Npgsql | `dotnet publish` | `ghcr.io/fatec-ads-ams2026/seniorcare-api` |
| `care-web` | React + Vite (npm) | `npm ci && build` → nginx | `.../seniorcare-care-web` |
| `stock-web` | React + Vite (npm) | idem | `.../seniorcare-stock-web` |
| `senior-portal` | React + Vite (npm) | idem | `.../seniorcare-senior-portal` |
| Postgres | dado stateful | — | `postgres:16-alpine` (upstream) |

## 4. Versionamento — CalVer + release train

- **Padrão:** `vAAAA.MM.PATCH` (ex.: `v2026.08.0`).
- **Toda imagem de um release compartilha a mesma tag** → o conjunto é coerente.
  Nunca se mistura `api:2026.08.1` com `care-web:2026.07.0` num mesmo deploy.
- **Tags produzidas:**
  - push em `main` → sem imagem publicada (CI só valida — ver `ci.yml`);
  - tag `v*` → `:AAAA.MM.PATCH` **+** `:latest`.

## 5. Pipeline de CI/CD (GitHub Actions)

- **`ci.yml`** (PR e push em `main`) — path filter por módulo, build/lint/audit. Não
  publica imagem; é gate de qualidade (ver também `docs/` do processo de revisão de PR).
- **`release.yml`** (tag `v*`) — builda e empurra as 4 imagens com a tag do release
  para o GHCR (`docker/build-push-action`, cache de layers), gera o **release
  manifest** (§6) e o anexa ao GitHub Release.
- **`ghcr-retention.yml`** (semanal + manual) — poda imagens de rastreio interno
  (`sha-*`, 90 dias/últimas 20) e órfãs sem tag (7 dias). Releases e `:latest` nunca
  expiram.

> **Autenticação CI → GHCR:** `GITHUB_TOKEN` com `packages: write` no job de release.

## 6. Release manifest — a unidade de deploy

Cada release publica `releases/AAAA.MM.PATCH.env`, que fixa cada imagem por tag **e**
por digest (o digest `@sha256` é a identidade imutável — a tag é legível, o digest é o
que preserva os bits distribuídos):

```env
# releases/2026.08.0.env — gerado pelo release.yml
RELEASE=2026.08.0
GIT_TAG=v2026.08.0
GIT_SHA=50269b65
BUILT_AT=2026-08-05T14:30:00Z
API_IMAGE=ghcr.io/fatec-ads-ams2026/seniorcare-api:2026.08.0@sha256:ab12…
CARE_WEB_IMAGE=ghcr.io/fatec-ads-ams2026/seniorcare-care-web:2026.08.0@sha256:cd34…
STOCK_WEB_IMAGE=ghcr.io/fatec-ads-ams2026/seniorcare-stock-web:2026.08.0@sha256:ef56…
SENIOR_PORTAL_IMAGE=ghcr.io/fatec-ads-ams2026/seniorcare-senior-portal:2026.08.0@sha256:gh78…
```

É o **único** arquivo que define "o que compõe esta entrega" — serve para o deploy,
para o **rollback** (re-apontar para o manifest anterior) e como registro imutável da
composição. O `deploy.sh` puxa **pelo digest**.

### Regras de imutabilidade
- Tag de release **nunca** é reescrita nem sobrescrita; correção = **nova** PATCH
  (`2026.08.1`), jamais rebuild da mesma tag.
- `main` não publica imagem — só as tags `v*` viram artefato de distribuição.

## 7. CD — entrega no servidor (pull-based)

`infra/deploy/docker-compose.yml` troca `build:` por `image:` — pinada pelo release
manifest. A stack é composta por três camadas:

```
infra/deploy/docker-compose.yml   (base — usa image:, não build:)
      +  releases/AAAA.MM.PATCH.env   (o que subir — versões pinadas)
      +  clients/<nome>/.env           (como subir aqui — segredos/paths/CORS)
```

`deploy.sh <versão>`:
1. `docker login ghcr.io` (opcional via `GHCR_USER`/`GHCR_TOKEN`);
2. `pg_dump` do banco antes de qualquer mudança (rede de segurança — migrations do EF
   Core **não** são auto-reversíveis);
3. seleciona `releases/<versão>.env` + `clients/<nome>/.env`;
4. `docker compose pull` → `docker compose up -d --remove-orphans`;
5. aguarda os **healthchecks** (`postgres` → `api` → frontends) ficarem *healthy*;
6. grava a versão corrente em `.state/current-release` (para `rollback` saber a
   anterior) e registra em `clients/<nome>/deploy-history.md`.

Ver detalhes de uso em [`infra/deploy/README.md`](../../infra/deploy/README.md).

## 8. Camada de operação (opcional)

`docker-compose.ops.yml` + `ops.sh` sobem, como projeto compose separado, ferramentas
de observabilidade que **não** fazem parte do núcleo pull-based: Homepage (hub de
atalhos), Portainer (painel), Dozzle (logs ao vivo), Caddy (ingress/TLS por
subdomínio) e um backup diário (`pg_dump` em `.zip`, cron interno). Nenhuma delas é
fonte da verdade do deploy — só observam. Detalhes em `infra/deploy/README.md`.

## 9. O que fica para depois (não implementado ainda)

- **Assinatura de imagem (cosign) + SBOM** — o pin por digest (§6) já cobre
  integridade; autenticidade de origem via cosign keyless (GitHub OIDC) fica para
  quando houver exigência concreta de auditoria de cadeia de suprimento.
- **Múltiplos ambientes/clientes** — a estrutura `clients/<nome>/` já suporta mais de
  um ambiente, mas hoje só existe o placeholder `clients/exemplo/`.
- **Observabilidade central (host/containers, uptime, alertas)** — a camada `ops`
  cobre logs e um painel; métricas de host/uptime centralizadas (Zabbix/Grafana ou
  equivalente) ficam para quando houver servidor real recebendo tráfego.
