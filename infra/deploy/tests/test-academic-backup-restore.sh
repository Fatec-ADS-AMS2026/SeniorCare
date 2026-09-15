#!/usr/bin/env bash
set -euo pipefail

name="seniorcare-academic-recovery-${RANDOM}-${RANDOM}"
volume="${name}-postgres"
dump=$(mktemp)
cleanup() {
  docker rm -f "$name" >/dev/null 2>&1 || true
  docker volume rm -f "$volume" >/dev/null 2>&1 || true
  rm -f "$dump"
}
trap cleanup EXIT

docker volume create "$volume" >/dev/null
docker run -d --name "$name" -e POSTGRES_PASSWORD=synthetic-password \
  -e POSTGRES_DB=db_seniorcare_academico -v "$volume:/var/lib/postgresql/data" \
  postgres:16-alpine >/dev/null

for _ in $(seq 1 30); do
  if docker exec "$name" psql -U postgres -d db_seniorcare_academico -tAc 'SELECT 1' >/dev/null 2>&1; then
    break
  fi
  sleep 1
done
docker exec "$name" psql -U postgres -d db_seniorcare_academico -tAc 'SELECT 1' >/dev/null

docker exec "$name" psql -U postgres -d db_seniorcare_academico -v ON_ERROR_STOP=1 \
  -c "CREATE TABLE academic_seed (id integer PRIMARY KEY, label text NOT NULL); INSERT INTO academic_seed VALUES (1, 'Pessoa Sintética 1');" >/dev/null
docker exec "$name" pg_dump -U postgres -d db_seniorcare_academico > "$dump"
docker exec "$name" psql -U postgres -d db_seniorcare_academico -v ON_ERROR_STOP=1 \
  -c "UPDATE academic_seed SET label = 'alterado';" >/dev/null
docker exec "$name" psql -U postgres -d postgres -v ON_ERROR_STOP=1 \
  -c "DROP DATABASE db_seniorcare_academico;" >/dev/null
docker exec "$name" psql -U postgres -d postgres -v ON_ERROR_STOP=1 \
  -c "CREATE DATABASE db_seniorcare_academico;" >/dev/null
docker exec -i "$name" psql -U postgres -d db_seniorcare_academico -v ON_ERROR_STOP=1 < "$dump" >/dev/null

[ "$(docker exec "$name" psql -U postgres -d db_seniorcare_academico -tAc 'SELECT label FROM academic_seed WHERE id = 1')" = 'Pessoa Sintética 1' ]
if docker exec "$name" pg_dump -U postgres -d inexistente >/dev/null 2>&1; then
  echo 'pg_dump de banco inexistente deveria falhar' >&2
  exit 1
fi
docker volume inspect "$volume" >/dev/null

echo 'OK: backup e restauração acadêmicos preservam dados sintéticos e uma falha não remove o volume.'
