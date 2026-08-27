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

O `.env.example` já vem com bootstrap e Mailpit preenchidos. No primeiro boot,
a API cria a instituição e o administrador `PROVISIONED`, sem senha, e envia o
link para `http://localhost:8025`. O Portal em `http://localhost:3002` conduz a
ativação e exibe QR code mais chave manual no cadastro obrigatório de MFA.

Para exercitar o fallback manual, deixe todas as chaves SMTP vazias no `.env`,
recrie o banco vazio e rode:

```bash
./bootstrap-dev-admin.sh
```

Sem `DEV_ADMIN_PASSWORD`, o helper gera uma senha efêmera forte e a mostra
apenas nessa execução; não há senha conhecida versionada. Pra ver os passos manuais
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
| senior-portal | 3002 |
| API | 8080 (`/health/live`, `/health/ready`) |
| Mailpit | 8025 (SMTP 1025) |
| Postgres | 5432 |
| pgAdmin (`--profile tools`) | 5050 |
