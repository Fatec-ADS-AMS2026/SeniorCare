# Spec Kit no GitHub — issues, PRs e kanban

Espelha as tarefas do GitHub Spec Kit no GitHub: cada seção de uma feature vira
issue, o trabalho acontece em branch com PR vinculado, e o merge fecha a issue.
O kanban é o [`SeniorCare Project`](https://github.com/orgs/Fatec-ADS-AMS2026/projects/2).

## Direção da verdade

O `tasks.md` de cada feature manda. A issue é espelho e seu corpo é **reescrito**
a cada sincronização. Marcar caixinha na issue não altera o repositório.

Para mudar o progresso, edite o `tasks.md` e faça merge na `main`.

## Escopo

Só as features listadas em
[`synced-features.txt`](synced-features.txt) são espelhadas. Use o slug sem o
prefixo numérico, por exemplo `introduce-senior-portal`; o sincronizador resolve
`specs/005-introduce-senior-portal/`.

Seções 100% concluídas não geram issue nova. Se uma issue existente ficar
completa, a sincronização a fecha; se uma tarefa reabrir, ela também reabre.

## Ciclo de trabalho

Para começar um épico com ID no título, por exemplo `CARE-EP03`:

```bash
./infra/speckit-github/start_epic.sh 11
```

O script cria `epic/care-ep03` a partir da `main` e abre um PR rascunho com
`Closes #11`. Ao concluir, marque as tarefas no `tasks.md`, retire o PR de
rascunho e faça merge. O push na `main` dispara
[`speckit-sync.yml`](../../.github/workflows/speckit-sync.yml).

## Sincronização manual

```bash
python3 infra/speckit-github/sync_issues.py --configured --dry-run
python3 infra/speckit-github/sync_issues.py --feature introduce-senior-portal
python3 infra/speckit-github/sync_issues.py --all --include-completed
```

O marcador HTML reconhece também issues produzidas antes da migração, evitando
duplicá-las. A execução seguinte troca o corpo pelo formato `speckit-sync` e
acrescenta os labels `spec-kit` e `feature:<slug>`.

## Kanban

```bash
gh auth refresh -s project
./infra/speckit-github/setup_project.sh
```

O script adiciona ao projeto as issues com label `spec-kit` e as coloca em
`Backlog`. As automações nativas do board continuam responsáveis por mover uma
issue fechada para `Done`.

## Limite conhecido

`start_epic.sh` exige um ID nomeado no título da seção. Seções apenas numeradas
continuam sincronizadas, mas sua branch precisa ser criada manualmente.
