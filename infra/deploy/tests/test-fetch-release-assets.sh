#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
HELPER="$SCRIPT_DIR/../fetch-release-assets.sh"
ROOT=$(mktemp -d)
trap 'rm -rf "$ROOT"' EXIT
VERSION=2026.09.0
SHA=0123456789abcdef0123456789abcdef01234567

fail() { echo "FALHA: $*" >&2; exit 1; }
pass() { echo "OK: $*"; }

make_manifest() {
  local destination="$1" release="${2:-$VERSION}" digest="${3:-$(printf 'a%.0s' {1..64})}"
  cat > "$destination/${VERSION}.env" <<EOF
RELEASE=$release
GIT_TAG=v$VERSION
GIT_SHA=$SHA
API_IMAGE=ghcr.io/example/seniorcare-api:$VERSION@sha256:$digest
CARE_WEB_IMAGE=ghcr.io/example/seniorcare-care-web:$VERSION@sha256:$digest
STOCK_WEB_IMAGE=ghcr.io/example/seniorcare-stock-web:$VERSION@sha256:$digest
SENIOR_PORTAL_IMAGE=ghcr.io/example/seniorcare-senior-portal:$VERSION@sha256:$digest
EOF
  printf 'BEGIN;\nCOMMIT;\n' > "$destination/${VERSION}-migration.sql"
}

FAKE_BIN="$ROOT/bin"
mkdir -p "$FAKE_BIN"
cat > "$FAKE_BIN/gh" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
[ "${FAKE_GH_MODE:-ok}" != interrupted ] || exit 1
while [ "$#" -gt 0 ]; do
  if [ "$1" = --dir ]; then
    destination="$2"
    break
  fi
  shift
done
mkdir -p "$destination"
cp "$FAKE_GH_ASSETS"/* "$destination/"
[ "${FAKE_GH_MODE:-ok}" != missing ] || rm -f "$destination"/*-migration.sql
EOF
chmod +x "$FAKE_BIN/gh"

run_helper() {
  PATH="$FAKE_BIN:$PATH" RELEASE_REPOSITORY=example/seniorcare \
    RELEASES_DIR="$1" FAKE_GH_ASSETS="$2" "$HELPER" "$VERSION"
}

assets="$ROOT/assets"
releases="$ROOT/releases"
mkdir -p "$assets"
make_manifest "$assets"
run_helper "$releases" "$assets"
[ "$(cat "$releases/${VERSION}.env")" = "$(cat "$assets/${VERSION}.env")" ] || fail "release completo não foi instalado"
pass "release completo"

if FAKE_GH_MODE=missing run_helper "$releases" "$assets"; then
  fail "asset ausente foi aceito"
fi
[ -f "$releases/${VERSION}-migration.sql" ] || fail "asset local válido foi removido"
pass "asset ausente"

bad_version="$ROOT/bad-version"
mkdir -p "$bad_version"
make_manifest "$bad_version" 2026.09.1
if run_helper "$releases" "$bad_version"; then
  fail "versão divergente foi aceita"
fi
pass "versão divergente"

bad_digest="$ROOT/bad-digest"
mkdir -p "$bad_digest"
make_manifest "$bad_digest" "$VERSION" invalid
if run_helper "$releases" "$bad_digest"; then
  fail "digest malformado foi aceito"
fi
pass "digest malformado"

before=$(shasum -a 256 "$releases/${VERSION}.env")
if FAKE_GH_MODE=interrupted run_helper "$releases" "$assets"; then
  fail "download interrompido foi aceito"
fi
after=$(shasum -a 256 "$releases/${VERSION}.env")
[ "$before" = "$after" ] || fail "download interrompido alterou asset local"
pass "download interrompido"
