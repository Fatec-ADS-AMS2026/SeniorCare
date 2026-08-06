#!/usr/bin/env bash
# Abre o trabalho de um épico: cria a branch a partir da main e um PR rascunho
# já vinculado à issue. O vínculo é a linha `Closes #N` no corpo do PR — é ela
# que faz o GitHub fechar a issue no merge. A branch é apagada automaticamente
# se `delete_branch_on_merge` estiver habilitado no repositório.
#
# Cobre apenas épicos com id nomeado no título (ex.: [CARE-EP03] ...). Seções
# sem id — como as do change `stabilize-existing-platform`, tituladas
# `[stabilize-existing-platform] §N ...` — não têm um slug estável para nomear
# a branch; crie-a à mão nesse caso (ex.: epic/stabilize-01-baseline).
#
# Uso: start_epic.sh <número-da-issue>
set -euo pipefail

REPO="Fatec-ADS-AMS2026/SeniorCare"

if [ $# -ne 1 ]; then
  echo "uso: $(basename "$0") <número-da-issue>" >&2
  exit 2
fi

ISSUE="$1"

TITLE=$(gh issue view "$ISSUE" --repo "$REPO" --json title --jq .title)
if [ -z "$TITLE" ]; then
  echo "issue #$ISSUE não encontrada" >&2
  exit 1
fi

# `[CARE-EP03] Sessão e contratos` -> `care-ep03`. O id do épico é estável;
# o título não, então a branch não depende dele.
EPIC_ID=$(printf '%s' "$TITLE" | sed -n 's/^\[\([A-Z][A-Z]*-EP[0-9][0-9]*\)\].*/\1/p' | tr '[:upper:]' '[:lower:]')
if [ -z "$EPIC_ID" ]; then
  echo "issue #$ISSUE não tem id de épico no título: $TITLE" >&2
  echo "esse fluxo cobre apenas épicos nomeados (ex.: [CARE-EP03] ...)" >&2
  exit 1
fi

BRANCH="epic/${EPIC_ID}"

if git show-ref --quiet "refs/heads/${BRANCH}"; then
  echo "branch ${BRANCH} já existe localmente" >&2
  exit 1
fi

git fetch --quiet origin main
git checkout --quiet -b "$BRANCH" origin/main
# Commit vazio apenas para permitir abrir o PR antes do primeiro código real.
git commit --quiet --allow-empty -m "chore(${EPIC_ID}): abre trabalho do épico

Refs #${ISSUE}"
git push --quiet -u origin "$BRANCH"

gh pr create \
  --repo "$REPO" \
  --base main \
  --head "$BRANCH" \
  --draft \
  --title "${TITLE}" \
  --body "$(cat <<EOF
Closes #${ISSUE}

Trabalho do épico rastreado na issue #${ISSUE}, espelho da seção correspondente
do \`tasks.md\` do change OpenSpec.

Ao concluir, marque as tarefas no \`tasks.md\` — é ele que manda. O merge deste
PR fecha a issue e apaga a branch; a sincronização seguinte na \`main\` atualiza
o progresso das demais issues.
EOF
)"

echo
echo "branch ${BRANCH} criada e PR rascunho aberto para a issue #${ISSUE}"
