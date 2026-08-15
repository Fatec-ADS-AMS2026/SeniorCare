## Why

A revisão semântica de pull requests depende hoje de uma GitHub Action da
Anthropic e de um token de assinatura armazenado no repositório. O projeto já
adotou revisão orientada por instruções versionadas em outro repositório; trazer
o mesmo modelo ao SeniorCare reduz dependência de fornecedor e mantém os
critérios de OpenSpec, segurança e LGPD auditáveis junto do código.

## What Changes

- Remover o workflow `claude-review` e sua dependência da Action da Anthropic.
- Transferir os critérios semânticos de revisão para `AGENTS.md`, em linguagem
  independente de provedor e adequada ao domínio do SeniorCare.
- Preservar os gates determinísticos obrigatórios e a proteção de `main`/`dev`.
- Remover do GitHub o secret `CLAUDE_CODE_OAUTH_TOKEN` depois que nenhuma
  referência versionada depender dele.
- **BREAKING**: o repositório deixa de publicar automaticamente o check e o
  comentário `claude-review`; a revisão semântica passa a depender do mecanismo
  externo autorizado que consome as instruções versionadas.

## Capabilities

### New Capabilities

Nenhuma.

### Modified Capabilities

- `automated-quality-gates`: torna a revisão semântica independente de provedor,
  preserva seus critérios no repositório e separa-a dos checks determinísticos
  obrigatórios.

## Impact

- **Domínio afetado:** engenharia, segurança da entrega e governança; nenhum
  fluxo assistencial nem dado de pessoa idosa é alterado.
- **Atores afetados:** mantenedores, revisores e administrador técnico da
  plataforma.
- **Código e sistemas:** `.github/workflows/claude-review.yml`, `AGENTS.md`,
  OpenSpec e secrets do GitHub Actions.
- **Risco assistencial/regulatório:** indireto e baixo; critérios explícitos de
  autenticação, autorização, LGPD e aderência às specs são preservados para não
  reduzir a qualidade da revisão.
- **Não objetivo:** substituir build, testes, SAST, SCA ou detecção de segredos,
  nem configurar um novo token de modelo dentro do GitHub Actions.
