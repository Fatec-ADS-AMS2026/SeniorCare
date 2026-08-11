# Tutorial: rodando e gerando os containers Docker

Guia pra subir o SeniorCare inteiro (Postgres + API + os dois front-ends) via
Docker, buildando as imagens a partir do código local. Pra rodar com debug
real via Rider/WebStorm em vez de container, veja
[`tutorial-desenvolvimento-ides.md`](tutorial-desenvolvimento-ides.md).

Existem **dois** compose diferentes no repo, pra propósitos diferentes — este
tutorial cobre o primeiro:

| | `infra/docker-test/` | `infra/deploy/` |
|---|---|---|
| Propósito | Desenvolvimento/teste local | Produção |
| Origem das imagens | **Builda do código local** (`build:` no compose) | **Puxa do GHCR** (pinadas por digest, nunca builda) |
| Cobertura deste tutorial | ✅ | Só a seção 5 |

## Pré-requisitos

- Docker + Docker Compose (`docker compose version` pra confirmar).

## 1. Subir tudo (build + run)

```bash
cd infra/docker-test
cp .env.example .env
# ajuste POSTGRES_PASSWORD pelo menos — as variáveis Bootstrap__* já vêm
# preenchidas com valores de exemplo, ajuste se quiser (opcional)

docker compose up -d --build
```

`--build` builda as 3 imagens (API, care-web, stock-web) a partir do
`Dockerfile` de cada componente antes de subir — na primeira vez demora mais
(baixa as imagens base, restaura dependências); nas próximas, o cache de
camadas do Docker acelera bastante.

Confirme que tudo subiu saudável:

```bash
docker compose ps
```

Todos os serviços devem aparecer como `healthy` (a API demora um pouco mais —
`start_period: 60s` no healthcheck — porque aplica as migrações do banco no
boot).

## 2. Portas

| Serviço | URL |
|---|---|
| care-web | http://localhost:3000 |
| stock-web | http://localhost:3001 |
| API | http://localhost:8080 (`/swagger`, `/health/live`, `/health/ready`) |
| Postgres | `localhost:5432` (acessível de fora, ex.: DBeaver/pgAdmin) |

## 3. Primeiro acesso (bootstrap do admin)

No primeiro boot (banco vazio), a API cria a instituição e o administrador
inicial a partir das variáveis `Bootstrap__*` do `.env` — o token de ativação
só aparece **uma vez** no log:

```bash
docker logs seniorcare-api 2>&1 | grep -A1 "Token de ativação"
```

Procedimento completo (capturar token, ativar conta, MFA obrigatório) em
[`../infra/deploy/BOOTSTRAP.md`](../infra/deploy/BOOTSTRAP.md).

## 4. Comandos do dia a dia

```bash
# rebuildar só um serviço depois de mudar código (ex.: backend)
docker compose up -d --build seniorcare-api

# logs ao vivo de um serviço
docker compose logs -f seniorcare-api

# entrar num container (debug)
docker exec -it seniorcare-api sh

# pgAdmin opcional (perfil "tools")
docker compose --profile tools up -d pgadmin   # http://localhost:5050

# parar tudo (mantém os dados do Postgres)
docker compose down

# parar e apagar os dados do Postgres também (recomeçar do zero)
docker compose down -v
```

## 5. Gerando imagens pra produção (visão geral)

O fluxo de produção é diferente — **nunca builda no servidor** (modelo
build-once/deploy-many). As imagens são geradas uma vez em CI
(`.github/workflows/release.yml`, disparado por uma tag `v*`), publicadas no
GHCR e só então puxadas pelo servidor via `infra/deploy/deploy.sh`. Se você
precisa gerar uma imagem de produção manualmente (situação incomum — o normal
é deixar o CI fazer isso), o mesmo `Dockerfile` de cada componente é a fonte
da verdade:

```bash
docker build -t seniorcare-api:local SeniorCareManager-Backend/SeniorCareManager.WebAPI
docker build -t seniorcare-care-web:local SeniorCareManager-Frontend/SeniorCareManagerFrontend
docker build -t seniorcare-stock-web:local SeniorStockManager-Frontend/SeniorStockManagerFrontend
```

Detalhes completos do fluxo de release/deploy em
[Arquitetura de CI/CD](infra/ci-cd-arquitetura.md) e
[`infra/deploy/README.md`](../infra/deploy/README.md).

## Documentação relacionada

- [Tutorial: rodando via Rider + WebStorm](tutorial-desenvolvimento-ides.md)
- [Bootstrap da instituição e do administrador inicial](../infra/deploy/BOOTSTRAP.md)
- [`infra/docker-test/README.md`](../infra/docker-test/README.md)
- [`infra/deploy/README.md`](../infra/deploy/README.md)
- [Arquitetura de CI/CD](infra/ci-cd-arquitetura.md)
