# Guia rápido do ambiente de desenvolvimento

Este guia prepara o SeniorCare para desenvolvimento local. Há dois modos de
execução; escolha **um** deles para a API e os front-ends:

| Modo | Indicado para | Onde a API recebe configuração |
|---|---|---|
| Docker Compose | subir a aplicação completa sem depurador | `infra/docker-test/.env` |
| Rider + WebStorm | depurar a API e usar hot reload nos front-ends | variáveis da Run Configuration do Rider |

> O arquivo `infra/docker-test/.env` é lido pelo Docker Compose. Ele **não** é
> carregado automaticamente quando a API é executada pelo Rider ou por
> `dotnet run` fora do contêiner.

## Opção A — aplicação completa com Docker

Pré-requisito: Docker Desktop em execução.

```bash
cd infra/docker-test
cp .env.example .env
# Edite .env e troque POSTGRES_PASSWORD por uma senha local.
docker compose up -d --build
docker compose ps
```

O `.env.example` já contém valores locais para `Bootstrap__*` e SMTP apontando
para o Mailpit. Após os serviços ficarem saudáveis, acesse:

- Portal e ativação: <http://localhost:3002>
- Gestão assistencial: <http://localhost:3000>
- Estoque: <http://localhost:3001>
- API/Swagger: <http://localhost:8080/swagger>
- Mailpit (e-mail de ativação): <http://localhost:8025>

Para encerrar, execute `docker compose down`. Para apagar os dados locais e
refazer o primeiro acesso, execute `docker compose down -v`.

## Opção B — Rider + WebStorm (debug e hot reload)

Use este modo quando quiser depurar o backend no Rider e ter atualização
automática (HMR) nos front-ends pelo WebStorm. API e front-ends ficam fora de
contêiner; somente Postgres e Mailpit usam Docker.

### Passo 1 — pré-requisitos

Tenha Docker Desktop, Rider, WebStorm e Node.js instalados. Confirme que o
Docker está em execução antes de continuar.

### Passo 2 — iniciar Postgres e Mailpit

```bash
cd infra/docker-test
cp .env.example .env
# Para este modo, use exatamente este valor: POSTGRES_PASSWORD=postdba
docker compose up -d postgres mailpit
```

A API em `Development` usa uma connection string local já configurada para essa
senha. Não aponte esse perfil para banco real.

Confirme que os dois contêineres foram iniciados:

```bash
docker compose ps
```

### Passo 3 — configurar e iniciar a API no Rider

1. Abra `SeniorCareManager-Backend/SeniorCareManager.WebAPI/SeniorCareManager.WebAPI.sln`.
2. Na Run Configuration da API, use `http://localhost:8080` ou configure os
   proxies dos front-ends para a porta que você escolher.
3. Antes do primeiro Debug, adicione estas variáveis **na Run Configuration do
   Rider**:

   ```text
   Bootstrap__InstitutionName=ILPI Dev
   Bootstrap__AdminEmail=admin@example.com
   Bootstrap__AdminDisplayName=Admin Dev
   Smtp__Host=localhost
   Smtp__Port=1025
   Smtp__FromAddress=noreply@seniorcare.local
   Smtp__FromDisplayName=SeniorCare Local
   Smtp__UseStartTls=false
   Frontend__ActivationBaseUrl=http://localhost:5173/ativar-conta
   ```

4. Execute em Debug. O Mailpit recebe o link de ativação em
   <http://localhost:8025>.

As três variáveis `Bootstrap__*` devem ser informadas juntas ou omitidas juntas.
Elas só criam a instituição e o administrador quando o banco ainda não tem
nenhuma instituição.

### Passo 4 — iniciar os front-ends no WebStorm

Abra cada diretório abaixo em uma janela separada do WebStorm:

1. `SeniorPortal-Frontend/SeniorPortalFrontend`
2. `SeniorCareManager-Frontend/SeniorCareManagerFrontend`
3. `SeniorStockManager-Frontend/SeniorStockManagerFrontend`

Em cada janela, instale as dependências uma única vez e execute a configuração
`npm: dev` que o WebStorm detectar. Alternativamente, use os terminais:

Em três terminais, um para cada projeto:

```bash
cd SeniorPortal-Frontend/SeniorPortalFrontend && npm install && npm run dev
cd SeniorCareManager-Frontend/SeniorCareManagerFrontend && npm install && npm run dev
cd SeniorStockManager-Frontend/SeniorStockManagerFrontend && npm install && npm run dev
```

Inicie o Portal primeiro; normalmente ele usa `http://localhost:5173`. Care e
Stock usarão as próximas portas livres. O proxy padrão dos front-ends aponta
para `http://localhost:8080`.

### Passo 5 — ativar o administrador e cadastrar MFA

1. Abra o Mailpit em <http://localhost:8025>.
2. Abra a mensagem de ativação enviada para `Bootstrap__AdminEmail` e siga o
   link para o Portal.
3. Escolha a senha do administrador na tela de ativação.
4. Faça login pelo Portal; o sistema redireciona para o cadastro obrigatório
   de MFA.
5. Escaneie o QR code em um aplicativo autenticador, confirme o código de seis
   dígitos e guarde os códigos de recuperação exibidos.

Ao concluir, o ambiente local estará pronto. Nas próximas execuções, basta
repetir os passos 2 a 4; a conta já ativa não é recriada.

## Onde configurar cada variável?

| Variável | Docker Compose | API executada no Rider |
|---|---|---|
| `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` | `infra/docker-test/.env` | continuam no `.env`, pois configuram o contêiner Postgres |
| `Bootstrap__InstitutionName`, `Bootstrap__AdminEmail`, `Bootstrap__AdminDisplayName` | `infra/docker-test/.env` | Run Configuration do Rider |
| `Smtp__Host`, `Smtp__Port`, `Smtp__Username`, `Smtp__Password`, `Smtp__FromAddress`, `Smtp__FromDisplayName`, `Smtp__UseStartTls` | `infra/docker-test/.env` | Run Configuration do Rider |
| `Frontend__ActivationBaseUrl` | `infra/docker-test/.env` | Run Configuration do Rider |
| `ASPNETCORE_ENVIRONMENT` | `infra/docker-test/.env` | o perfil de desenvolvimento do Rider já usa `Development` |

Portanto, **não**: no modo Rider, configurar `Bootstrap__*`, `Smtp__*` e
`Frontend__ActivationBaseUrl` somente no `.env` não basta. Elas precisam estar
nas variáveis de ambiente da Run Configuration (ou ser fornecidas por outro
provedor de configuração local da API). Não grave segredos em arquivos
`appsettings.*` versionados.

## Primeiro acesso e senha do administrador

Não há senha padrão. No primeiro boot, a API cria o administrador como
`PROVISIONED`, sem senha. Abra o link recebido no Mailpit e escolha uma senha
forte; em seguida, conclua o cadastro obrigatório de MFA.

Sem SMTP, a API exibe um token de ativação uma única vez no console. Com esse
token, você pode automatizar o fluxo local:

```bash
cd infra/docker-test
DEV_ADMIN_EMAIL=admin@example.com ./bootstrap-dev-admin.sh --token <token>
```

Defina `DEV_ADMIN_PASSWORD` somente se quiser escolher a senha usada pelo
helper. Se ela for omitida, o helper gera uma senha forte efêmera e a mostra
apenas durante a execução.

## Referências

- [Tutorial detalhado: Rider + WebStorm](tutorial-desenvolvimento-ides.md)
- [Tutorial detalhado: Docker](tutorial-docker.md)
- [Configuração da API](../SeniorCareManager-Backend/SeniorCareManager.WebAPI/CONFIGURATION.md)
- [Bootstrap da instituição e do administrador](../infra/deploy/BOOTSTRAP.md)
