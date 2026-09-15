## Context

O workflow `claude-review.yml` executa uma revisão semântica em cada PR não
rascunho por meio de `anthropics/claude-code-action`, usando
`CLAUDE_CODE_OAUTH_TOKEN`. Os checks protegidos de `main` e `dev` são
determinísticos e não incluem `claude-review`. O precedente do QualitasSystem
removeu o workflow equivalente e preservou as regras de revisão em `AGENTS.md`.

## Goals / Non-Goals

**Goals:**

- Eliminar a dependência versionada e o secret da Anthropic sem alterar os gates
  determinísticos obrigatórios.
- Tornar os critérios semânticos legíveis por pessoas e ferramentas autorizadas.
- Preservar o foco específico do SeniorCare em OpenSpec, autorização progressiva,
  LGPD e dados de pessoas idosas.

**Non-Goals:**

- Criar outro workflow de modelo ou armazenar uma nova credencial de IA no GitHub.
- Alterar branch protection, CI, SAST, SCA, gitleaks ou política de merge.
- Garantir por código do repositório que uma integração externa esteja habilitada.

## Decisions

### 1. Remover o workflow inteiro

O workflow é exclusivo da Anthropic; mantê-lo desabilitado deixaria código morto,
permissão de escrita em PR e referências ao secret. A alternativa de trocar apenas
o modelo continuaria acoplando a revisão a uma Action e credencial de provedor.

### 2. Versionar regras semânticas em `AGENTS.md`

As regras serão extraídas do prompt atual e condensadas em critérios normativos:
objetivo do PR e OpenSpec, autorização e IDOR, LGPD, vulnerabilidades, correção e
qualidade do parecer. Elas não mencionarão Claude, Codex ou outro modelo.

### 3. Manter revisão semântica fora dos required checks

`main` e `dev` continuarão exigindo `ci-required`, `gitleaks-pr`, CodeQL C# e
JavaScript/TypeScript e `security-sca-required`. Isso evita que indisponibilidade
de uma integração externa bloqueie a entrega, sem reduzir os gates reprodutíveis.

### 4. Remover o secret somente após eliminar referências

Primeiro o workflow é removido e uma busca confirma que nenhuma referência a
`CLAUDE_CODE_OAUTH_TOKEN` ou à Action permanece. Só então o secret é excluído no
GitHub. Essa ordem evita quebrar uma execução ainda versionada.

## Risks / Trade-offs

- **[Revisão semântica externa não habilitada]** → as regras permanecem disponíveis
  para revisão humana; a integração externa deve ser verificada separadamente, sem
  reintroduzir segredo de modelo no Actions.
- **[Perda de detalhe ao condensar o prompt]** → preservar todos os blocos de risco
  e os critérios de achado acionável, eliminando apenas instruções específicas da
  Action.
- **[Rollback exige nova credencial]** → reverter os arquivos não recupera um secret
  apagado; uma eventual volta à Anthropic exigiria gerar uma nova credencial.

## Migration Plan

1. Adicionar as regras independentes de provedor ao `AGENTS.md`.
2. Remover `.github/workflows/claude-review.yml`.
3. Confirmar ausência de referências Anthropic e preservar os required checks.
4. Remover `CLAUDE_CODE_OAUTH_TOKEN` do GitHub Actions.
5. Validar OpenSpec e atualizar o Graphify.

Rollback: reverter os arquivos e, apenas se houver decisão explícita de retornar ao
provedor anterior, gerar uma nova credencial e cadastrá-la com escopo mínimo.
