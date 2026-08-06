# OpenSpec no GitHub — issues, PRs e kanban

Espelha a stack do OpenSpec no GitHub: cada épico de um change vira issue, o
trabalho acontece em branch com PR vinculado, e o merge fecha a issue e apaga a
branch. O kanban é o GitHub Project já usado pelo time
([`SeniorCare Project`](https://github.com/orgs/Fatec-ADS-AMS2026/projects/2)),
alimentado pelas mesmas issues.

## Direção da verdade

O `tasks.md` de cada change manda. A issue é espelho e seu corpo é **reescrito**
a cada sincronização — marcar caixinha na issue não altera o repositório e some
na próxima execução. Isso é deliberado: o estado de trabalho continua versionado
no git, revisável em PR, sem commits de bot nem conflito entre dois lados
editáveis.

Para mudar o progresso, edite o `tasks.md` e faça merge na `main`.

## Escopo

Só os changes listados em [`synced-changes.txt`](synced-changes.txt) são
espelhados. Para incluir outro, acrescente a linha e faça merge; o push
seguinte cria as issues das seções com trabalho aberto.

Seções 100% concluídas não geram issue nova. Se uma issue existente ficar
completa, a sincronização a fecha; se uma tarefa reabrir no `tasks.md`, ela
reabre.

## Ciclo de trabalho

Começar um épico (só funciona para seções com id nomeado no título, ex.
`[CARE-EP03] ...` — ver limites conhecidos abaixo):

```bash
./infra/openspec-github/start_epic.sh 11
```

Cria a branch `epic/care-ep03` a partir da `main` e abre um PR rascunho com
`Closes #11` no corpo. É essa linha que faz o GitHub fechar a issue no merge.

Ao concluir, marque as tarefas no `tasks.md`, tire o PR de rascunho e mergeie.
No merge o GitHub fecha a issue e apaga a branch (se `delete_branch_on_merge`
estiver habilitado no repositório). O push na `main` dispara o
[`openspec-sync.yml`](../../.github/workflows/openspec-sync.yml), que atualiza o
progresso das demais issues.

## Sincronização manual

```bash
python3 infra/openspec-github/sync_issues.py --configured --dry-run
python3 infra/openspec-github/sync_issues.py --change stabilize-existing-platform
python3 infra/openspec-github/sync_issues.py --all --include-completed
```

O `--dry-run` mostra o que seria criado ou atualizado sem tocar no GitHub. A
reconciliação entre execuções usa um marcador HTML no corpo da issue
(`<!-- openspec-sync: change=... section=... -->`), não o título — renomear um
épico não duplica a issue.

## Kanban

```bash
gh auth refresh -s project   # uma vez, abre o navegador
./infra/openspec-github/setup_project.sh
```

O script adiciona as issues com label `openspec` ao `SeniorCare Project`
(reaproveita o project existente — não cria um novo) e as coloca em "Backlog".

No `SeniorCare Project` (board #2), as automações nativas já estão ligadas
(verificado em 2026-08-06: fechar issue → Done; adicionar item com label
`openspec` → entra sozinho e já em Backlog). Se um dia precisar reconferir ou
reconfigurar — outro project, ou alguém desligou por engano — isso fica em
Settings → Workflows do project, na interface; não há equivalente na CLI.
`gh issue merged` de PR → Done não foi testado (exigiria abrir um PR real
contra a `main` protegida só para o teste).

## Limites conhecidos

O fluxo do `start_epic.sh` cobre apenas épicos com id nomeado (`CARE-EP03`,
`STOCK-EP01`). Changes cujas seções são só numeradas sem id — caso do
`stabilize-existing-platform` hoje — geram issue normalmente, mas a branch
precisa ser criada à mão, porque não há id estável para nomeá-la.
