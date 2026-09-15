# Tutorial: rodando e gerando os containers Docker

Guia pra subir o SeniorCare inteiro (Postgres + Mailpit + API + os três front-ends) via
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

`--build` builda as 4 imagens locais (API, Portal, care-web e stock-web) a partir do
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
| senior-portal | http://localhost:3002 |
| API | http://localhost:8080 (`/swagger`, `/health/live`, `/health/ready`) |
| Mailpit | http://localhost:8025 (SMTP em `localhost:1025`) |
| Postgres | `localhost:5432` (acessível de fora, ex.: DBeaver/pgAdmin) |

## 3. Primeiro login (criar e ativar o usuário admin)

A API não vem com nenhum usuário/senha padrão — no primeiro boot (banco
vazio), ela cria a instituição e o administrador a partir das três variáveis
`Bootstrap__*` do `.env` (já preenchidas no `.env.example`, ver seção 1). Não
existe senha inicial padrão: a conta nasce `PROVISIONED`, sem senha, e a própria
pessoa define a credencial no link de ativação.

Por padrão, o compose configura a API para o Mailpit. Abra
`http://localhost:8025`, selecione a mensagem e siga o link para o Portal. Quando
a entrega funciona, o token não aparece na saída de `seniorcare-api`.

### 3.1. Caminho rápido no fallback manual — script

O helper trabalha com o token apresentado pela API, portanto use-o quando SMTP
estiver desabilitado ou a entrega falhar. Para testar esse modo, defina **todas**
estas chaves vazias no `.env`, recrie o banco vazio e suba a stack:

```dotenv
Smtp__Host=
Smtp__Port=
Smtp__Username=
Smtp__Password=
Smtp__FromAddress=
Smtp__FromDisplayName=
Smtp__UseStartTls=
Frontend__ActivationBaseUrl=
```

```bash
./bootstrap-dev-admin.sh
```

Faz tudo de uma vez: espera a API ficar pronta, captura o token do log,
ativa a conta, loga e cadastra o MFA calculando o TOTP sozinho. O e-mail vem
de `DEV_ADMIN_EMAIL`; a senha deve vir de `DEV_ADMIN_PASSWORD` não versionada
ou é gerada com alta entropia e exibida somente nessa execução. Idempotente —
rode de novo quantas vezes quiser, ele reconhece o que já foi feito. Ao
final, imprime e-mail/senha e a chave do autenticador (salva localmente em
`.dev-admin-mfa-key`, não versionada, só pra esse script recalcular o código
em execuções futuras).

Não existe nenhuma senha conhecida ou versionada no helper. Ele não contorna
nem enfraquece o MFA — automatiza exatamente os mesmos passos
que um humano faria via curl (seção 3.2), só sem precisar copiar/colar nada.

### 3.2. Passo a passo manual (o que o script acima faz por baixo)

**a. Obter a ativação.** No modo padrão, use o link recebido no Mailpit. No
fallback, o token aparece **uma única vez** no log do boot com banco vazio:

```bash
docker logs seniorcare-api 2>&1 | grep "Token de ativação"
```

O banco guarda somente o hash. No ambiente local ainda não ativado, use
`docker compose down -v` e suba de novo; não existe consulta de recuperação.

**b. Ativar a conta.** Abra o link no Portal (`http://localhost:3002`) ou, no
fallback, a rota `/ativar-conta`; preencha e-mail, token e a senha que você
escolheu. Ou use diretamente a API:

```bash
curl -X POST http://localhost:8080/api/v1/Auth/activate \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","token":"<token>","newPassword":"<senha-forte>"}'
```

**c. Logar e cadastrar o MFA (obrigatório, sem exceção pro bootstrap).** Pelo
front-end: vá em `http://localhost:3000/login`, entre com o e-mail/senha do
passo b — o sistema redireciona automaticamente pra `/mfa/enroll` (todo login
administrativo, inclusive o primeiro, exige MFA cadastrado). A tela mostra um
QR code e a chave manual equivalente; escaneie o QR num app autenticador ou
cadastre a chave em texto, então digite o código de 6 dígitos. Depois de
confirmar, guarde os 10
códigos de recuperação mostrados (opcional, cada um só serve uma vez) — login
completo, você cai direto no painel.

Pra fazer o mesmo fluxo só por API/curl calculando o código TOTP você mesmo
(sem celular, sem abrir o navegador), é exatamente o que `bootstrap-dev-admin.sh`
(seção 3.1) já faz — abra o script se quiser ver os comandos `curl` exatos.

### 3.3. Administradores criados depois do bootstrap

A tela de usuários mostra se a ativação foi enviada. Se aparecer falha, corrija
o bloco SMTP, recrie somente o container da API e use **Reenviar ativação** na
conta `PROVISIONED`. A ação invalida o token anterior e nunca o expõe; o banco
armazena somente hashes, portanto consulta direta não é um fallback válido.

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
