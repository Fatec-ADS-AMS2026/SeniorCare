#!/usr/bin/env bash
# Baixa os dois assets obrigatórios de uma GitHub Release para uso pelo deploy.
# A validação ocorre no diretório temporário; assets locais só mudam depois que
# manifesto e migration da mesma versão foram baixados e aceitos.
set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
REPO_ROOT=$(cd "$SCRIPT_DIR/../.." && pwd)
RELEASES_DIR=${RELEASES_DIR:-"$REPO_ROOT/releases"}

err() { printf 'ERRO: %s\n' "$*" >&2; }
die() { err "$*"; exit 1; }

usage() {
  echo "uso: $(basename "$0") <versao>" >&2
  exit 2
}

repository_from_origin() {
  local origin
  origin=$(git -C "$REPO_ROOT" config --get remote.origin.url 2>/dev/null || true)
  case "$origin" in
    git@github.com:*.git) printf '%s\n' "${origin#git@github.com:}" | sed 's/\.git$//' ;;
    https://github.com/*.git) printf '%s\n' "${origin#https://github.com/}" | sed 's/\.git$//' ;;
    https://github.com/*) printf '%s\n' "${origin#https://github.com/}" ;;
    *) die "defina RELEASE_REPOSITORY=owner/repo; não consegui inferir o repositório GitHub de origin" ;;
  esac
}

value() {
  local key="$1" file="$2" matches
  matches=$(grep -c "^${key}=" "$file" || true)
  [ "$matches" -eq 1 ] || die "$file deve conter exatamente uma chave $key"
  sed -n "s/^${key}=//p" "$file"
}

validate_manifest() {
  local version="$1" manifest="$2" migration="$3" key image
  [ -s "$manifest" ] || die "manifesto ausente ou vazio: $manifest"
  [ -s "$migration" ] || die "script de migração ausente ou vazio: $migration"
  [ "$(value RELEASE "$manifest")" = "$version" ] || die "manifesto não corresponde à versão solicitada: $version"
  [ "$(value GIT_TAG "$manifest")" = "v$version" ] || die "manifesto tem GIT_TAG divergente"
  [[ "$(value GIT_SHA "$manifest")" =~ ^[0-9a-f]{40}$ ]] || die "manifesto tem GIT_SHA inválido"

  for key in API_IMAGE CARE_WEB_IMAGE STOCK_WEB_IMAGE SENIOR_PORTAL_IMAGE; do
    image=$(value "$key" "$manifest")
    [[ "$image" =~ ^ghcr\.io/.+:[^@]+@sha256:[0-9a-f]{64}$ ]] || die "imagem inválida para $key"
  done
}

main() {
  [ "$#" -eq 1 ] || usage
  local version="$1" repository manifest migration tmp
  [[ "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+([.-][0-9A-Za-z.-]+)?$ ]] || die "versão inválida: $version"
  command -v gh >/dev/null || die "gh CLI não encontrado; instale-o antes do deploy"

  repository=${RELEASE_REPOSITORY:-$(repository_from_origin)}
  manifest="${version}.env"
  migration="${version}-migration.sql"
  mkdir -p "$RELEASES_DIR"
  tmp=$(mktemp -d "$RELEASES_DIR/.${version}.download.XXXXXX")
  trap 'rm -rf "${tmp:-}"' EXIT

  gh release download "v${version}" --repo "$repository" \
    --pattern "$manifest" --pattern "$migration" --dir "$tmp"
  validate_manifest "$version" "$tmp/$manifest" "$tmp/$migration"

  # Ambos arquivos foram validados antes de qualquer substituição do cache local.
  mv "$tmp/$manifest" "$RELEASES_DIR/$manifest"
  mv "$tmp/$migration" "$RELEASES_DIR/$migration"
  printf 'Assets do release %s validados em %s\n' "$version" "$RELEASES_DIR"
}

main "$@"
