#!/usr/bin/env bash
# Adiciona ao GitHub Project da organização todas as issues com o label
# `openspec`, e as coloca no status "Backlog". Idempotente: rodar de novo só
# acrescenta o que faltar. Reaproveita o project já existente do time — não
# cria um novo.
#
# Pré-requisito de escopo — o token do gh precisa de acesso a Projects:
#   gh auth refresh -s project
#
# Uso: setup_project.sh
set -euo pipefail

REPO="Fatec-ADS-AMS2026/SeniorCare"
OWNER="Fatec-ADS-AMS2026"
PROJECT_TITLE="SeniorCare Project"
STATUS_FIELD_NAME="Status"
BACKLOG_OPTION_NAME="Backlog"

if ! gh project list --owner "$OWNER" --limit 1 >/dev/null 2>&1; then
  echo "token do gh sem escopo de Projects. Rode:" >&2
  echo "  gh auth refresh -s project" >&2
  exit 1
fi

NUMBER=$(gh project list --owner "$OWNER" --format json --limit 100 \
  | python3 -c "
import json, sys
title = '''$PROJECT_TITLE'''
data = json.load(sys.stdin)
for project in data.get('projects', []):
    if project.get('title') == title:
        print(project['number'])
        break
")

if [ -z "$NUMBER" ]; then
  echo "project '$PROJECT_TITLE' não encontrado no owner $OWNER" >&2
  echo "ajuste PROJECT_TITLE neste script ou crie o project antes de rodar" >&2
  exit 1
fi
echo "project #$NUMBER ($PROJECT_TITLE)"

PROJECT_ID=$(gh project view "$NUMBER" --owner "$OWNER" --format json --jq .id)

# Resolve o id do campo Status e da opção Backlog — precisa para setar o status
# do item recém-adicionado (item novo entra sem status até uma automação da
# UI, ainda não configurada, ou este script, setar).
read -r STATUS_FIELD_ID BACKLOG_OPTION_ID < <(
  gh project field-list "$NUMBER" --owner "$OWNER" --format json --limit 200 \
  | python3 -c "
import json, sys
field_name = '''$STATUS_FIELD_NAME'''
option_name = '''$BACKLOG_OPTION_NAME'''
data = json.load(sys.stdin)
for f in data.get('fields', []):
    if f.get('name') == field_name:
        for opt in f.get('options', []):
            if opt.get('name') == option_name:
                print(f['id'], opt['id'])
                sys.exit(0)
sys.exit('campo/opção não encontrados')
"
)

# Itens já no board, para não duplicar.
EXISTING=$(gh project item-list "$NUMBER" --owner "$OWNER" --format json --limit 500 \
  | python3 -c "
import json, sys
data = json.load(sys.stdin)
for item in data.get('items', []):
    content = item.get('content') or {}
    if content.get('url'):
        print(content['url'])
")

gh issue list --repo "$REPO" --label openspec --state all --limit 200 --json url --jq '.[].url' \
| while read -r url; do
  if printf '%s\n' "$EXISTING" | grep -qxF "$url"; then
    continue
  fi
  echo "  + $url"
  ITEM_ID=$(gh project item-add "$NUMBER" --owner "$OWNER" --url "$url" --format json --jq .id)
  gh project item-edit \
    --project-id "$PROJECT_ID" \
    --id "$ITEM_ID" \
    --field-id "$STATUS_FIELD_ID" \
    --single-select-option-id "$BACKLOG_OPTION_ID" >/dev/null
done

cat <<EOF

Board: https://github.com/orgs/${OWNER}/projects/${NUMBER}

Falta habilitar as automações nativas, que não têm equivalente na CLI e são
configuradas na interface do próprio project, em Settings -> Workflows:

  - "Item closed"        -> mover para Done
  - "Pull request merged"-> mover para Done
  - "Auto-add to project"-> filtro: is:issue label:openspec

Com a terceira ligada, todo épico novo criado pelo sync_issues.py entra no
board sozinho (mas ainda sem status — configure também "Item added to
project" -> Status: Backlog, se quiser que a colocação em Backlog também seja
automática; senão, rode este script de novo depois de cada sincronização).
EOF
