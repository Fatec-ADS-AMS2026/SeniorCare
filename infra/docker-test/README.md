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

## Portas (default)

| Serviço | Porta host |
|---|---|
| care-web | 3000 |
| stock-web | 3001 |
| API | 8080 (`/health/live`, `/health/ready`) |
| Postgres | 5432 |
| pgAdmin (`--profile tools`) | 5050 |
