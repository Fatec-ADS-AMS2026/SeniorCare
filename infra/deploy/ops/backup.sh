#!/bin/sh
# Backup diário do SeniorCare: dump do banco, empacotado em UM .zip por dia.
# Executado pelo cron do serviço `backup` (docker-compose.ops.yml). Complementa o
# gate pré-deploy do deploy.sh.
#
# Se o SeniorCare ganhar armazenamento de arquivo (fotos de residentes, documentos)
# no futuro, monte o(s) diretório(s) sob /data no compose — este script já compacta
# tudo que encontrar lá, sem precisar mudar.
#
# Vars (de /etc/backup.env, derivado do ambiente do container):
#   POSTGRES_HOST/DB/USER/PASSWORD  — conexão com o banco
#   BACKUP_KEEP_DAYS                — retenção (default 14 dias)
# Saída: /backups/seniorcare-backup-AAAAMMDD.zip

set -eu

OUT_DIR=/backups
DATA_DIR=/data                 # diretórios de dados eventualmente montados aqui (RO)
TS=$(date +%Y%m%d)
OUT="$OUT_DIR/seniorcare-backup-${TS}.zip"
PART="${OUT}.partial"
TMP=$(mktemp -d)
DUMP="$TMP/database-${TS}.sql"

log() { echo "[backup $(date +%H:%M:%S)] $*"; }
cleanup() { rm -rf "$TMP" "$PART" 2>/dev/null || true; }
trap cleanup EXIT

# 1) Dump do banco (aborta o backup inteiro se falhar — nada de zip corrompido).
log "pg_dump ${POSTGRES_DB}@${POSTGRES_HOST} ..."
PGPASSWORD="$POSTGRES_PASSWORD" pg_dump -h "$POSTGRES_HOST" -U "$POSTGRES_USER" -d "$POSTGRES_DB" -f "$DUMP"

# 2) Monta o .zip do dia (dump na raiz; cada diretório de dados sob seu nome, se houver).
rm -f "$PART"
log "compactando dump ..."
( cd "$TMP" && zip -q "$PART" "database-${TS}.sql" )

found_any=0
for d in "$DATA_DIR"/*/; do
  [ -d "$d" ] || continue
  name=$(basename "$d")
  log "compactando dados: $name ..."
  ( cd "$DATA_DIR" && zip -qr "$PART" "$name" )
  found_any=1
done
[ "$found_any" -eq 1 ] || log "sem diretório de dados em $DATA_DIR — só o banco no zip"

# Publica de forma atômica (só vira .zip final quando tudo deu certo).
mv "$PART" "$OUT"
log "gerado: $OUT ($(du -h "$OUT" | cut -f1))"

# 3) Retenção: remove os .zip mais antigos que BACKUP_KEEP_DAYS.
KEEP="${BACKUP_KEEP_DAYS:-14}"
log "retenção: apagando zips com mais de ${KEEP} dias"
find "$OUT_DIR" -maxdepth 1 -name 'seniorcare-backup-*.zip' -type f -mtime +"$KEEP" -print -delete || true

log "concluído."
