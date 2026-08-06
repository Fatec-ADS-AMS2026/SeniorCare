#!/usr/bin/env bash
# promote_to_main.sh — abre o PR de dev -> main depois que a equipe terminou
# de testar o que foi mergeado em dev.
#
# `Closes #N` no corpo fecha a issue de QA no merge. A decisão de "terminei de
# testar" é sempre humana — este script só mecaniza a parte chata (template,
# Closes #N, checar se a issue já foi fechada).
#
# Uso: promote_to_main.sh <número-da-issue-de-qa>
set -euo pipefail

REPO="Fatec-ADS-AMS2026/SeniorCare"

if [ $# -ne 1 ]; then
  echo "uso: $(basename "$0") <número-da-issue-de-qa>" >&2
  exit 2
fi

ISSUE="$1"

IFS=$'\t' read -r TITLE STATE < <(gh issue view "$ISSUE" --repo "$REPO" --json title,state --jq '"\(.title)\t\(.state)"')
if [ -z "$TITLE" ]; then
  echo "issue #$ISSUE não encontrada" >&2
  exit 1
fi

if [ "$STATE" != "CLOSED" ]; then
  echo "aviso: issue #$ISSUE (\"$TITLE\") ainda está aberta." >&2
  read -r -p "tem certeza que o teste terminou? [y/N] " confirm
  case "$confirm" in
    y|Y|yes|s|S|sim) ;;
    *) echo "abortado."; exit 1 ;;
  esac
fi

git fetch --quiet origin dev main

# Remove o prefixo "[QA] " do título da issue (que já embute o título/número do
# PR original mergeado em dev) para compor o título do PR de promoção.
PR_TITLE="Promove dev -> main: ${TITLE#\[QA\] }"

# Corpo do PR vai por arquivo (não por heredoc dentro de $(...)) — bash 3.2
# (padrão do macOS) tem um bug de parsing conhecido nessa combinação.
BODY_FILE=$(mktemp)
trap 'rm -f "$BODY_FILE"' EXIT
cat > "$BODY_FILE" <<EOF
Closes #${ISSUE}

Promoção de \`dev\` para \`main\` após teste manual concluído (issue #${ISSUE}).

Os checks de CI já rodaram em cada PR individual que entrou em \`dev\`; este PR
consolida o que foi testado. Revisar o diff \`main...dev\` antes de aprovar.
EOF

gh pr create \
  --repo "$REPO" \
  --base main \
  --head dev \
  --title "$PR_TITLE" \
  --body-file "$BODY_FILE"

echo
echo "PR de promoção aberto (dev -> main)."
