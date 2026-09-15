# Handoff — release e operação via Portainer

_Data: 2026-09-15._

## Estado entregue

- Release válida publicada: [`v2026.09.2`](https://github.com/Fatec-ADS-AMS2026/SeniorCare/releases/tag/v2026.09.2).
- Commit da release: `c116a03b2df00ff4248f80b0d6b0e7aca35ce8d4`.
- Branches `main` e `dev` estão sincronizadas nesse commit.
- GitHub Actions CI e SAST passaram antes da publicação da tag.
- O workflow `release.yml` passou integralmente: valida a origem em `main`, publica as quatro imagens no GHCR, gera a migração e cria o GitHub Release.

## Artefatos da release

A release contém:

- `2026.09.2.env` — manifest com todas as imagens pinadas por tag e digest;
- `2026.09.2-migration.sql` — migração SQL idempotente.

As imagens publicadas são:

- `ghcr.io/fatec-ads-ams2026/seniorcare-api:2026.09.2`
- `ghcr.io/fatec-ads-ams2026/seniorcare-care-web:2026.09.2`
- `ghcr.io/fatec-ads-ams2026/seniorcare-stock-web:2026.09.2`
- `ghcr.io/fatec-ads-ams2026/seniorcare-senior-portal:2026.09.2`

O deploy deve usar os valores com `@sha256` existentes no manifest, não a tag `latest`.

## Modelo de responsabilidade

| Etapa | Responsável |
|---|---|
| Qualidade, testes, SAST e validação acadêmica | GitHub Actions (`ci.yml`) |
| Build das imagens, publicação GHCR e criação do release manifest | GitHub Actions (`release.yml`, disparado por tag `v*`) |
| Aplicação da release no servidor | `infra/deploy/deploy.sh` |
| Visibilidade operacional, logs e gestão dos containers | Portainer/Dozzle na camada `ops` |

**Decisão vigente:** o CD é pull-based no servidor. Portainer é painel operacional; não é fonte de verdade nem substitui o manifest de release ou o `deploy.sh`. Isso está definido em `docs/infra/ci-cd-arquitetura.md` §§6–8 e `infra/deploy/README.md`.

## Próxima retomada: configurar servidor e Portainer

1. Provisionar servidor com Docker e Docker Compose.
2. Clonar o repositório e criar `infra/deploy/clients/<ambiente>/.env` a partir do exemplo, mantendo segredos somente no servidor.
3. Configurar acesso de leitura ao GHCR com token de escopo mínimo; não usar credenciais pessoais de desenvolvimento.
4. Executar o bootstrap descrito em `infra/deploy/BOOTSTRAP.md` antes do primeiro deploy.
5. Baixar os assets da versão a promover:

   ```bash
   cd infra/deploy
   ./fetch-release-assets.sh v2026.09.2
   ```

6. Aplicar a versão usando o orquestrador versionado:

   ```bash
   export CLIENT=<ambiente>
   ./deploy.sh 2026.09.2
   ```

   O script realiza login no GHCR quando configurado, backup PostgreSQL, pull por digest, subida da stack, health checks e histórico de deploy.

7. Subir a camada operacional separada, se desejada:

   ```bash
   cp .env.ops.example .env.ops
   # preencher domínio, portas e credenciais
   ./ops.sh up
   ```

   Portainer ficará em `https://<host>:9443` por padrão. Restringir essa porta por firewall ou expor apenas via rede/VPN administrativa, pois ele acessa o socket Docker.

## Operação de versões

- Promover uma versão significa baixar o manifest daquele release e executar `deploy.sh <versão>`.
- Rollback significa `./deploy.sh rollback`, que reutiliza o manifest da versão anterior.
- Nunca configurar a stack produtiva para `latest`.
- Nunca reescrever uma tag publicada. Correções exigem nova tag PATCH.
- As tags `v2026.09.0` e `v2026.09.1` foram tentativas falhas de pipeline; a primeira release válida é `v2026.09.2`.

## Ajustes feitos no pipeline nesta entrega

- Corrigida a referência ao job `senior-portal` no agregador de CI.
- Normalizado para minúsculas o namespace do GHCR, obrigatório para os nomes de imagem Docker.
- Garantida a criação de `_manifest` antes de gravar os fragmentos de imagens.
