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
inicial. Pra ativar a conta e cadastrar o MFA (obrigatório, e sem QR code —
ver pendências em [`../deploy/BOOTSTRAP.md`](../deploy/BOOTSTRAP.md#pendências-conhecidas-leia-antes-de-operar-em-produção))
sem fazer nada manualmente:

```bash
./bootstrap-dev-admin.sh
```

Idempotente, pode rodar de novo a qualquer momento. Pra ver os passos manuais
que esse script automatiza (útil pra debugar ou entender o fluxo), veja o
[tutorial de Docker](../../docs/tutorial-docker.md#3-primeiro-login-criar-e-ativar-o-usuário-admin)
ou [`../deploy/BOOTSTRAP.md`](../deploy/BOOTSTRAP.md) (procedimento de
referência — os passos 2-4 de lá valem igual aqui, só troca
`./deploy.sh <versão>` por `docker compose up -d --build`).

## Portas (default)

| Serviço | Porta host |
|---|---|
| care-web | 3000 |
| stock-web | 3001 |
| API | 8080 (`/health/live`, `/health/ready`) |
| Postgres | 5432 |
| pgAdmin (`--profile tools`) | 5050 |
