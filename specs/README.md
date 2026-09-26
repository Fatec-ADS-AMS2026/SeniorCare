# Especificações do SeniorCare

Este diretório é a fonte de verdade do planejamento no GitHub Spec Kit. A migração
de OpenSpec para Spec Kit foi feita em 2026-09-26 sem alterar o estado das tarefas.

## Mapeamento

| Feature Spec Kit | Origem migrada | Estado |
|---|---|---|
| `001-stabilize-existing-platform` | mudança arquivada e cinco specs canônicas | concluída |
| `002-migrate-pr-review-from-anthropic` | mudança arquivada | concluída |
| `003-improve-first-access-delivery` | mudança ativa com tarefas concluídas | convergência final pendente |
| `004-add-academic-homologation-environment` | mudança ativa | em andamento |
| `005-introduce-senior-portal` | mudança ativa | em andamento |

Cada feature usa `spec.md`, `plan.md` e `tasks.md`. Os IDs `Tnnn` seguem o
formato do Spec Kit; o identificador anterior aparece como `(legado N.N)` para
preservar links e rastreabilidade histórica.

## Uso

Os comandos oficiais estão instalados para OMP, Codex e Claude. O fluxo normal é:

```text
/speckit.specify → /speckit.plan → /speckit.tasks → /speckit.implement → /speckit.converge
```

Para trabalhar numa feature migrada sem trocar de branch, selecione seu diretório:

```bash
export SPECIFY_FEATURE_DIRECTORY=specs/005-introduce-senior-portal
.specify/scripts/bash/check-prerequisites.sh --json --require-spec --require-tasks --include-tasks
```

Features novas recebem o próximo prefixo numérico disponível. Requisitos
`MUST`/`SHALL`, cenários `WHEN`/`THEN` e tarefas concluídas permanecem contratos de
revisão bloqueantes.
