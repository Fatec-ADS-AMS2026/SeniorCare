# Tutorial: rodando o SeniorCare em desenvolvimento (Rider + WebStorm)

Guia passo a passo para rodar a API e os dois front-ends **fora de container**,
com debug real e hot reload, usando Rider (backend) e WebStorm (front-ends).
Para rodar tudo via Docker (sem instalar SDK/Node localmente), veja
[`tutorial-docker.md`](tutorial-docker.md).

## Pré-requisitos

- [Rider](https://www.jetbrains.com/rider/) (ou qualquer IDE JetBrains com
  suporte a .NET) — .NET SDK 8.0.x.
- [WebStorm](https://www.jetbrains.com/webstorm/) — Node.js 22.
- Docker (só pra subir o Postgres — não precisa containerizar a API nem os
  front-ends pra este fluxo).

## 1. Banco de dados

A API em `Development` (perfil padrão quando você roda pela IDE) lê a
connection string de `appsettings.Development.json`, que já vem com host/porta/
senha fixos — **só funciona contra o Postgres do `infra/docker-test/`**, nunca
aponta pra um ambiente real (ver
[`CONFIGURATION.md`](../SeniorCareManager-Backend/SeniorCareManager.WebAPI/CONFIGURATION.md)).

```bash
cd infra/docker-test
cp .env.example .env
# IMPORTANTE: a senha do Postgres tem que ser exatamente "postdba" — é o
# valor fixo em appsettings.Development.json. Edite o .env recém-criado:
#   POSTGRES_PASSWORD=postdba
docker compose up -d postgres
```

Isso sobe só o Postgres (não a API nem os front-ends em container — vamos
rodar os três pela IDE). Confirme que subiu: `docker ps` deve listar
`seniorcare-postgres`.

## 2. Backend — Rider

1. Abra `SeniorCareManager-Backend/SeniorCareManager.WebAPI/SeniorCareManager.WebAPI.sln`
   no Rider (ele carrega os 3 projetos: `WebAPI`, `UnitTests`, `IntegrationTests`).
2. **Ajuste a configuração de execução antes do primeiro run** — o perfil
   `https` padrão (`Properties/launchSettings.json`) sobe em
   `https://localhost:7053;http://localhost:5253`, mas o proxy `/api` dos dois
   front-ends espera `http://localhost:8080` por padrão (mesma porta que o
   Docker usa). Duas opções, escolha uma:
   - **Mais simples**: edite a Run Configuration gerada pelo Rider (ícone de
     lápis ao lado do seletor de configuração) e mude a URL da aplicação pra
     `http://localhost:8080`.
   - **Alternativa**: mantenha a porta padrão e, ao rodar os front-ends (passo
     3), defina `VITE_API_PROXY_TARGET=http://localhost:5253` no `.env` de
     cada um.
3. Rode em modo **Debug** (▷ com o ícone de inseto, ou `Shift+F9`) — a
   variável `ASPNETCORE_ENVIRONMENT=Development` já vem definida no perfil.
   Antes desse primeiro run, veja a seção 4 abaixo — ela cobre a variável de
   bootstrap que precisa estar definida ANTES de rodar, pra API criar a
   instituição/admin inicial no boot.
4. Swagger abre automaticamente (`launchUrl: swagger` no perfil) — confirma
   que a API está de pé em `http://localhost:8080/swagger` (ou a porta que
   você configurou).
5. **Testes**: o Rider descobre os testes xUnit automaticamente nos projetos
   `UnitTests`/`IntegrationTests` — rode pela aba de testes (ícone de tubo de
   ensaio) ou clique com o botão direito no projeto → Run Tests. Os testes de
   integração sobem um Postgres efêmero próprio via Testcontainers — não usam
   o Postgres do passo 1, e precisam do Docker rodando.

## 3. Front-ends — WebStorm

Cada front-end é um projeto independente (não há workspace/monorepo do
npm) — abra cada um como uma janela separada do WebStorm:
`SeniorCareManager-Frontend/SeniorCareManagerFrontend` (app "care") e
`SeniorStockManager-Frontend/SeniorStockManagerFrontend` (app "stock").

Para cada um:

1. Abra a pasta no WebStorm. Ele detecta o `package.json` e oferece rodar
   `npm install` automaticamente (aceite, ou rode `npm install` no terminal
   integrado).
2. **Run Configuration**: WebStorm cria configurações `npm` a partir dos
   scripts do `package.json` — use `dev` (servidor Vite com HMR,
   `npm run dev`). Rode com o botão ▷ ou `Ctrl+R`/`Cmd+R`.
   - Care abre em `http://localhost:5173` (porta padrão do Vite — confirme no
     terminal integrado, o Vite escolhe outra se a 5173 estiver ocupada).
   - Se os dois apps rodarem ao mesmo tempo, o Vite do segundo sobe numa porta
     diferente automaticamente (ex.: 5174) — sem conflito.
3. O proxy `/api` (configurado em `vite.config.ts`) já encaminha pra
   `http://localhost:8080` por padrão — ajuste conforme o passo 2 do backend
   se você mudou a porta lá.
4. **Lint**: `npm run lint` (ou a integração nativa de ESLint do WebStorm,
   ativa por padrão quando detecta a config do projeto).
5. **Testes**: WebStorm reconhece o Vitest automaticamente (ícone ▷ ao lado de
   cada `describe`/`it` nos arquivos `*.test.tsx`) — ou rode `npm test` no
   terminal integrado. Os testes usam mock de `@/features/api` (nunca chamam
   a API real), então não precisam do backend rodando.

## 4. Primeiro login (criar e ativar o usuário admin)

Com Postgres + API + pelo menos um front-end rodando, falta criar a conta que
você vai usar pra logar — a API não vem com nenhum usuário/senha padrão.

> **Pendência conhecida**: não existe serviço de e-mail nem geração de QR code
> nesta plataforma ainda — o token de ativação só existe no log/console, e o
> MFA só oferece a chave em texto pra digitar manualmente. Detalhe completo em
> [`../infra/deploy/BOOTSTRAP.md`](../infra/deploy/BOOTSTRAP.md#pendências-conhecidas-leia-antes-de-operar-em-produção).

**a. Definir as variáveis de bootstrap antes de subir a API.** No Rider, edite
a Run Configuration da API (ícone de lápis) → aba **Environment variables** →
adicione as três juntas (ou edite `appsettings.Development.json` — nunca
commite valor real lá, é só pro seu ambiente local):

```
Bootstrap__InstitutionName=ILPI Dev
Bootstrap__AdminEmail=admin@example.com
Bootstrap__AdminDisplayName=Admin Dev
```

Elas só têm efeito enquanto **nenhuma instituição existir no banco** — se seu
Postgres local já tem dado de uma sessão anterior, ou apague o volume
(`docker compose down -v` no `infra/docker-test`) ou pule pra "e" com a conta
que você já tem.

**b. Rodar a API (Debug) e capturar o token.** No primeiro boot com banco
vazio, o console do Rider imprime uma linha assim **uma única vez**:

```
Bootstrap: instituição e administrador PROVISIONED criados.
  Token de ativação (uso único, capture agora — não será reimpresso): <token>
```

Copie o `<token>` — se perder, não tem como recuperar pela API (só
reprovisionando a conta direto no banco).

**c. Ativar + logar + cadastrar MFA — caminho rápido (script).** Com o
token do passo b em mãos, um único comando faz o resto (ativação, login,
cadastro de MFA com TOTP calculado sozinho, sem celular):

```bash
cd infra/docker-test
DEV_ADMIN_EMAIL=admin@example.com ./bootstrap-dev-admin.sh --token <token>
```

(`--token` porque o backend aqui não está em container — sem ele, o script
tentaria ler o log de um container `seniorcare-api` que não existe nesse
fluxo.) Idempotente — pode rodar de novo sem `--token` nas próximas vezes,
ele usa a chave de MFA salva em `infra/docker-test/.dev-admin-mfa-key`.

Prefere fazer manualmente (ou entender o que o script faz por baixo)? Os
passos "d" e "e" abaixo são o equivalente manual, pela UI.

**d. Ativar a conta pelo front-end (manual).** Com o care (ou stock) rodando, abra
`http://localhost:5173/ativar-conta` (ajuste a porta se o Vite escolheu
outra) e preencha e-mail (`admin@example.com`), o token do passo b, e a senha
que você quer usar. Confirme "Conta ativada com sucesso."

**e. Logar e cadastrar o MFA (obrigatório pra toda conta administrativa).**
Vá em `/login`, entre com o e-mail/senha que você acabou de definir — o
sistema redireciona automaticamente pra `/mfa/enroll` (nenhum login
administrativo completa sem MFA cadastrado, nem no primeiro acesso). A tela
mostra uma chave (`authenticatorKey`) e o `otpauth://` correspondente:

- **Com celular à mão**: adicione uma conta manual num app autenticador
  (Google Authenticator, Authy, 1Password etc.) usando essa chave, e digite o
  código de 6 dígitos que ele gerar no campo "Código de confirmação".
- **Sem celular / fluxo scriptável**: gere o código você mesmo a partir da
  chave (TOTP padrão, SHA1/6 dígitos/30s) — por exemplo com Python (biblioteca
  padrão, sem instalar nada):

  ```python
  import base64, hmac, hashlib, struct, time
  def totp(secret_b32):
      key = base64.b32decode(secret_b32.upper() + '=' * (-len(secret_b32) % 8))
      msg = struct.pack('>Q', int(time.time() // 30))
      h = hmac.new(key, msg, hashlib.sha1).digest()
      o = h[-1] & 0x0f
      return str((struct.unpack('>I', h[o:o+4])[0] & 0x7fffffff) % 10**6).zfill(6)
  print(totp("<authenticatorKey>"))
  ```

Depois de confirmar, a tela mostra 10 códigos de recuperação (guarde se
quiser — cada um só serve uma vez) e te leva direto pro painel — login
completo.

Nas próximas vezes (conta já ativa, MFA já cadastrado), é só `/login` com
e-mail/senha + o código do autenticador (`/login/mfa`).

## 5. Fluxo do dia a dia

Com Postgres (Docker) + API (Rider, Debug) + os dois front-ends (WebStorm,
`npm run dev`) rodando, você tem o ambiente completo com debug real no
backend e hot reload nos dois front-ends — sem rebuildar imagem Docker a cada
mudança. Pare tudo com `docker compose down` (no `infra/docker-test`) quando
terminar; os dados do Postgres ficam num volume persistente entre sessões
(`docker compose down -v` apaga, se quiser recomeçar do zero).

## Documentação relacionada

- [Tutorial: rodando via Docker](tutorial-docker.md)
- [Bootstrap da instituição e do administrador inicial](../infra/deploy/BOOTSTRAP.md)
- [Configuração da API](../SeniorCareManager-Backend/SeniorCareManager.WebAPI/CONFIGURATION.md)
- [README do backend](../SeniorCareManager-Backend/README.md)
- [README do care](../SeniorCareManager-Frontend/SeniorCareManagerFrontend/README.md)
- [README do stock](../SeniorStockManager-Frontend/SeniorStockManagerFrontend/README.md)
