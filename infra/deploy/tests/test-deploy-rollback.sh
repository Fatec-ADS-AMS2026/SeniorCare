#!/usr/bin/env bash
set -euo pipefail
ROOT=$(mktemp -d)
trap 'rm -rf "$ROOT"' EXIT
DEPLOY="$ROOT/infra/deploy"
mkdir -p "$DEPLOY/clients/academico" "$ROOT/bin"
cp infra/deploy/deploy.sh infra/deploy/fetch-release-assets.sh infra/deploy/docker-compose.yml infra/deploy/docker-compose.homolog.yml "$DEPLOY/"
chmod +x "$DEPLOY/deploy.sh" "$DEPLOY/fetch-release-assets.sh"
printf '%s\n' \
  'POSTGRES_DB=db_seniorcare_academico' \
  'POSTGRES_USER=seniorcare_academico' \
  'POSTGRES_PASSWORD=synthetic-password' \
  'ACADEMIC_BACKUPS_PATH=/tmp/seniorcare-academico-backups' \
  > "$DEPLOY/clients/academico/.env"
LOG="$ROOT/docker.calls"
cat > "$ROOT/bin/gh" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
version=${3#v}
for ((i = 1; i <= $#; i++)); do
  if [ "${!i}" = --dir ]; then
    j=$((i + 1)); dir=${!j}; break
  fi
done
mkdir -p "$dir"
cat > "$dir/$version.env" <<MANIFEST
RELEASE=$version
GIT_TAG=v$version
GIT_SHA=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
API_IMAGE=ghcr.io/example/api:$version@sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
CARE_WEB_IMAGE=ghcr.io/example/care:$version@sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
STOCK_WEB_IMAGE=ghcr.io/example/stock:$version@sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
SENIOR_PORTAL_IMAGE=ghcr.io/example/portal:$version@sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
MANIFEST
printf 'BEGIN;\nCOMMIT;\n' > "$dir/$version-migration.sql"
EOF
cat > "$ROOT/bin/docker" <<EOF
#!/usr/bin/env bash
set -euo pipefail
release=''
for ((i = 1; i <= \$#; i++)); do
  if [ "\${!i}" = --env-file ]; then
    j=\$((i + 1)); candidate=\${!j}
    if [ -f "\$candidate" ] && grep -q '^RELEASE=' "\$candidate"; then release=\$(sed -n 's/^RELEASE=//p' "\$candidate"); fi
  fi
done
printf '%s|%s\n' "\$release" "\$*" >> "$LOG"
EOF
chmod +x "$ROOT/bin/gh" "$ROOT/bin/docker"
run() { PATH="$ROOT/bin:$PATH" RELEASE_REPOSITORY=owner/repo CLIENT=academico "$DEPLOY/deploy.sh" "$1"; }
run 1.0.0
run 1.1.0
run status
PATH="$ROOT/bin:$PATH" docker compose -p seniorcare-academico logs --tail 20
run rollback
[ "$(cat "$DEPLOY/.state/current-release")" = 1.0.0 ]
[ "$(cat "$DEPLOY/.state/previous-release")" = 1.1.0 ]
grep -Fq '1.0.0|compose ' "$LOG"
grep -Fq '|compose -p seniorcare-academico logs --tail 20' "$LOG"
! grep -Fqi portainer "$LOG"
! grep -Eq '(^| )down( |$).* -v|(^| )rm( |$)' "$LOG"
echo 'OK: deploy, status, logs e rollback funcionam sem Portainer e preservam o volume.'
