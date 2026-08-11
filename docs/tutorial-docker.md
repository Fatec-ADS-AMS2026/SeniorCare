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

## 3. Primeiro login (criar e ativar o usuário admin)

A API não vem com nenhum usuário/senha padrão — no primeiro boot (banco
vazio), ela cria a instituição e o administrador a partir das três variáveis
`Bootstrap__*` do `.env` (já preenchidas no `.env.example`, ver seção 1).

> **Pendência conhecida**: não existe serviço de e-mail nem geração de QR code
> nesta plataforma ainda — o token de ativação só existe no log do container,
> e o MFA só oferece a chave em texto pra digitar manualmente. É exatamente
> essa lacuna que o script da seção 3.1 abaixo automatiza pro ambiente de dev
> (não é uma correção da lacuna em si — produção continua precisando do
> procedimento manual). Detalhe completo em
> [`../infra/deploy/BOOTSTRAP.md`](../infra/deploy/BOOTSTRAP.md#pendências-conhecidas-leia-antes-de-operar-em-produção).

### 3.1. Caminho rápido — script

```bash
./bootstrap-dev-admin.sh
```

Faz tudo de uma vez: espera a API ficar pronta, captura o token do log,
ativa a conta (`admin@example.com` / `DevSenhaForte!2026` por padrão —
ajustável via `DEV_ADMIN_EMAIL`/`DEV_ADMIN_PASSWORD`), loga, e cadastra o MFA
calculando o código TOTP sozinho (sem celular, sem QR code). Idempotente —
rode de novo quantas vezes quiser, ele reconhece o que já foi feito. Ao
final, imprime e-mail/senha e a chave do autenticador (salva localmente em
`.dev-admin-mfa-key`, não versionada, só pra esse script recalcular o código
em execuções futuras).

Não contorna nem enfraquece o MFA — automatiza exatamente os mesmos passos
que um humano faria via curl (seção 3.2), só sem precisar copiar/colar nada.

### 3.2. Passo a passo manual (o que o script acima faz por baixo)

**a. Capturar o token de ativação.** Aparece **uma única vez** no log, no
boot com banco vazio:

```bash
docker logs seniorcare-api 2>&1 | grep "Token de ativação"
```

Se perder o token antes de ativar, não tem como recuperar pela API — só
reprovisionando a conta direto no banco, ou derrubando tudo com
`docker compose down -v` e subindo de novo do zero.

**b. Ativar a conta.** Pelo front-end: abra `http://localhost:3000/ativar-conta`
(care) e preencha e-mail (`admin@example.com`, ou o que você definiu no
`.env`), o token do passo a, e a senha que você quer usar. Ou direto pela API:

```bash
curl -X POST http://localhost:8080/api/v1/Auth/activate \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","token":"<token>","newPassword":"<senha-forte>"}'
```

**c. Logar e cadastrar o MFA (obrigatório, sem exceção pro bootstrap).** Pelo
front-end: vá em `http://localhost:3000/login`, entre com o e-mail/senha do
passo b — o sistema redireciona automaticamente pra `/mfa/enroll` (todo login
administrativo, inclusive o primeiro, exige MFA cadastrado). A tela mostra
uma chave (`authenticatorKey`); adicione uma conta manual num app
autenticador (Google Authenticator, Authy, 1Password etc.) com essa chave e
digite o código de 6 dígitos gerado. Depois de confirmar, guarde os 10
códigos de recuperação mostrados (opcional, cada um só serve uma vez) — login
completo, você cai direto no painel.

Pra fazer o mesmo fluxo só por API/curl calculando o código TOTP você mesmo
(sem celular, sem abrir o navegador), é exatamente o que `bootstrap-dev-admin.sh`
(seção 3.1) já faz — abra o script se quiser ver os comandos `curl` exatos.

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
