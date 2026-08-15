---
type: "query"
date: "2026-08-15T15:38:04.563332+00:00"
question: "Revise o projeto e verifique as pendências a partir do graphify e do openspec, preparando a retirada do modelo Anthropic na validação de PRs"
contributor: "graphify"
outcome: "useful"
source_nodes: ["Operação do Senior Portal", "1. Capacidade de envio de e-mail ()", "AuthController", "Program", "pull_request_template.md"]
---

# Q: Revise o projeto e verifique as pendências a partir do graphify e do openspec, preparando a retirada do modelo Anthropic na validação de PRs

## Answer

Expanded from original query via graph vocab: [claude, github, model, openspec, pull, tasks, validation, senior, portal, catalog, module, session, rollback, activation, authentication, bootstrap, delivery, email, mail, mfa, notification, recover, smtp]. OpenSpec tem duas changes ativas e válidas: introduce-senior-portal com quatro tarefas operacionais de homologação, aceite e corte; improve-first-access-delivery com vinte tarefas de implementação e aceite ainda abertas. Antes de implementar first access, atualizar artifacts para incluir o Senior Portal no QR/MFA, reconciliar SMTP por instituição com configuração global por ambiente e resolver a contradição sobre token de bootstrap em log. A revisão Anthropic está isolada em .github/workflows/claude-review.yml e usa CLAUDE_CODE_OAUTH_TOKEN; os required checks de main/dev não incluem claude-review. O precedente QualitasSystem efbe368d removeu esse workflow e transferiu regras semânticas para AGENTS.md.

## Outcome

- Signal: useful

## Source Nodes

- Operação do Senior Portal
- 1. Capacidade de envio de e-mail ()
- AuthController
- Program
- pull_request_template.md