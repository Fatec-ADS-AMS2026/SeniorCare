---
description: "Tarefas migradas para 002-migrate-pr-review-from-anthropic"
---

# Tasks: Migrar a revisão de PR para regras independentes de provedor

**Input**: [spec.md](spec.md) e [plan.md](plan.md)
**Status**: Concluída
**Formato**: IDs `Tnnn` são nativos do Spec Kit; `(legado N.N)` preserva as
referências usadas pela documentação e pelas issues existentes.

## 1. Política versionada e workflow

- [x] T001 (legado 1.1) Adicionar ao `AGENTS.md` regras de revisão semântica independentes de
      provedor, cobrindo entrega/Spec Kit, autenticação e autorização progressivas,
      IDOR, LGPD, vulnerabilidades, correção e qualidade do parecer.
- [x] T002 (legado 1.2) Remover `.github/workflows/claude-review.yml` sem alterar os workflows
      determinísticos existentes.

## 2. Configuração do GitHub

- [x] T003 (legado 2.1) Confirmar que os required checks de `main` e `dev` permanecem
      `ci-required`, `gitleaks-pr`, CodeQL C#, CodeQL JavaScript/TypeScript e
      `security-sca-required`, sem dependência de `claude-review`.
- [x] T004 (legado 2.2) Confirmar que nenhuma referência versionada depende da Anthropic e remover
      o secret `CLAUDE_CODE_OAUTH_TOKEN` do GitHub Actions.

## 3. Validação e documentação derivada

- [x] T005 (legado 3.1) Executar busca de referências, validação estrita do artefato legado, executada antes da migração e `git diff --check`.
- [x] T006 (legado 3.2) Atualizar o Graphify após as alterações versionadas e verificar que o
      grafo incorpora a nova política e a remoção do workflow.
