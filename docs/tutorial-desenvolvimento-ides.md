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
   No primeiro boot com banco vazio, a API bootstrapa a instituição/admin
   inicial — capture o token de ativação impresso no console do Rider (ver
   [`../infra/deploy/BOOTSTRAP.md`](../infra/deploy/BOOTSTRAP.md) pro
   procedimento completo).
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

## 4. Fluxo do dia a dia

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
