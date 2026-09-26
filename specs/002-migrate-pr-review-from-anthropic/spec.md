# Feature Specification: Migrar a revisão de PR para regras independentes de provedor

**Feature ID**: `002-migrate-pr-review-from-anthropic`
**Created**: 2026-09-26 (data da migração)
**Status**: Concluída
**Input**: artefatos legados incorporados nesta feature durante a migração

> Este documento preserva requisitos e cenários verificáveis existentes. A migração
> não inventa prioridades P1/P2/P3 retroativas; os cenários associados a cada
> requisito continuam sendo os testes de aceitação autoritativos.

## User Scenarios & Testing

Os fluxos verificáveis estão expressos como cenários `WHEN`/`THEN` sob cada
requisito. Para trabalho novo, o fluxo Spec Kit deve acrescentar histórias
priorizadas e testes independentes sem enfraquecer estes contratos existentes.

## Contexto e escopo

### Why

A revisão semântica de pull requests depende hoje de uma GitHub Action da
Anthropic e de um token de assinatura armazenado no repositório. O projeto já
adotou revisão orientada por instruções versionadas em outro repositório; trazer
o mesmo modelo ao SeniorCare reduz dependência de fornecedor e mantém os
critérios de Spec Kit, segurança e LGPD auditáveis junto do código.

### What Changes

- Remover o workflow `claude-review` e sua dependência da Action da Anthropic.
- Transferir os critérios semânticos de revisão para `AGENTS.md`, em linguagem
  independente de provedor e adequada ao domínio do SeniorCare.
- Preservar os gates determinísticos obrigatórios e a proteção de `main`/`dev`.
- Remover do GitHub o secret `CLAUDE_CODE_OAUTH_TOKEN` depois que nenhuma
  referência versionada depender dele.
- **BREAKING**: o repositório deixa de publicar automaticamente o check e o
  comentário `claude-review`; a revisão semântica passa a depender do mecanismo
  externo autorizado que consome as instruções versionadas.

### Capabilities

#### New Capabilities

Nenhuma.

#### Modified Capabilities

- `automated-quality-gates`: torna a revisão semântica independente de provedor,
  preserva seus critérios no repositório e separa-a dos checks determinísticos
  obrigatórios.

### Impact

- **Domínio afetado:** engenharia, segurança da entrega e governança; nenhum
  fluxo assistencial nem dado de pessoa idosa é alterado.
- **Atores afetados:** mantenedores, revisores e administrador técnico da
  plataforma.
- **Código e sistemas:** `.github/workflows/claude-review.yml`, `AGENTS.md`,
  Spec Kit e secrets do GitHub Actions.
- **Risco assistencial/regulatório:** indireto e baixo; critérios explícitos de
  autenticação, autorização, LGPD e aderência às specs são preservados para não
  reduzir a qualidade da revisão.
- **Não objetivo:** substituir build, testes, SAST, SCA ou detecção de segredos,
  nem configurar um novo token de modelo dentro do GitHub Actions.

## Requirements

### Capacidade `automated-quality-gates`

_Contrato de capacidade migrado sem alteração normativa._

### Functional Requirements

#### Requirement: Revisão semântica é versionada e independente de provedor
O repositório SHALL manter critérios de revisão semântica versionados e
independentes de um modelo específico. Esses critérios SHALL cobrir aderência ao
Spec Kit, autorização, proteção de dados pessoais, segurança e qualidade dos
achados. Os checks determinísticos obrigatórios SHALL permanecer separados da
revisão semântica e SHALL NOT depender de credencial de assinatura de modelo no
GitHub Actions.

##### Scenario: Revisor autorizado analisa um pull request
- **WHEN** um mecanismo autorizado realiza revisão semântica do pull request
- **THEN** ele encontra no repositório as regras de entrega, Spec Kit,
  autorização, LGPD, segurança e qualidade que devem orientar o parecer

##### Scenario: Pull request executa checks obrigatórios
- **WHEN** um pull request é aberto contra `main` ou `dev`
- **THEN** build, testes, SAST, SCA e detecção de segredos continuam conclusivos
  sem depender da execução de um modelo de linguagem

##### Scenario: Credencial do provedor removido
- **WHEN** não existe mais workflow versionado que use a integração da Anthropic
- **THEN** o repositório não referencia nem mantém o secret de assinatura desse
  provedor no GitHub Actions

## Success Criteria

A feature é aceita quando todos os cenários normativos acima são satisfeitos, as
tarefas marcadas como concluídas possuem implementação e evidência correspondentes
e os gates definidos na constituição do projeto passam. Tarefas ainda abertas em
`tasks.md` permanecem bloqueantes para a conclusão operacional da feature.

## Assumptions

- O escopo canônico e as decisões em aberto permanecem em
  [`docs/escopo-do-projeto.md`](../../docs/escopo-do-projeto.md).
- A API continua sendo a autoridade final de autorização.
- Nenhuma migração de formato altera o estado real das tarefas ou declara
  funcionalidades adicionais como entregues.
