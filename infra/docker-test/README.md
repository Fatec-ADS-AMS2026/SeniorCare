# infra/docker-test — stack local (build a partir do código)

Compose de **desenvolvimento**: builda as imagens a partir do código local (não
consome GHCR). Use para rodar o SeniorCare completo localmente ou testar as imagens
antes de um release. Para o compose de **produção** (pull-based, imagens do GHCR),
ver [`infra/deploy/`](../deploy/).

## Uso

```bash
cp .env.example .env    # ajuste POSTGRES_PASSWORD pelo menos
docker compose up -d --build

# opcional — pgAdmin:
docker compose --profile tools up -d pgadmin
```

O `.env.example` já vem com as três variáveis `Bootstrap__*` preenchidas — no
primeiro boot (banco vazio) a API cria a instituição e o administrador
inicial. Capture o token de ativação impresso uma única vez no log e ative a
conta: procedimento completo (captura do token, ativação, MFA) em
[`../deploy/BOOTSTRAP.md`](../deploy/BOOTSTRAP.md) — os passos 2 e 3 de lá
valem igual aqui, só troca `./deploy.sh <versão>` por `docker compose up -d --build`.

```bash
docker logs seniorcare-api 2>&1 | grep -A1 "Token de ativação"
```

## Portas (default)

| Serviço | Porta host |
|---|---|
| care-web | 3000 |
| stock-web | 3001 |
| API | 8080 (`/health/live`, `/health/ready`) |
| Postgres | 5432 |
| pgAdmin (`--profile tools`) | 5050 |
