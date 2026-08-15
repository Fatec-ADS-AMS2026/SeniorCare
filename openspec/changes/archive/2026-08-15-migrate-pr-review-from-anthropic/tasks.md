## 1. Política versionada e workflow

- [x] 1.1 Adicionar ao `AGENTS.md` regras de revisão semântica independentes de
      provedor, cobrindo entrega/OpenSpec, autenticação e autorização progressivas,
      IDOR, LGPD, vulnerabilidades, correção e qualidade do parecer.
- [x] 1.2 Remover `.github/workflows/claude-review.yml` sem alterar os workflows
      determinísticos existentes.

## 2. Configuração do GitHub

- [x] 2.1 Confirmar que os required checks de `main` e `dev` permanecem
      `ci-required`, `gitleaks-pr`, CodeQL C#, CodeQL JavaScript/TypeScript e
      `security-sca-required`, sem dependência de `claude-review`.
- [x] 2.2 Confirmar que nenhuma referência versionada depende da Anthropic e remover
      o secret `CLAUDE_CODE_OAUTH_TOKEN` do GitHub Actions.

## 3. Validação e documentação derivada

- [x] 3.1 Executar busca de referências, `openspec validate
      migrate-pr-review-from-anthropic --strict` e `git diff --check`.
- [x] 3.2 Atualizar o Graphify após as alterações versionadas e verificar que o
      grafo incorpora a nova política e a remoção do workflow.
