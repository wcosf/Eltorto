#!/bin/bash

# -------------------------------------------------------------
# БЭКАП БАЗЫ ДАННЫХ ELTORTO
# Место хранения: /var/backups/eltorto/
# Хранение: 7 дней (старые удаляются)
# Права: только root (700 для папки, 600 для файлов)
# Логи: /var/log/backup_db.log

BACKUP_DIR="/var/backups/eltorto"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
KEEP_DAYS=7
DB_NAME="eltorto_pg"
DB_USER="postgres"
CONTAINER="eltorto_postgres"
LOG_FILE="/var/log/backup_db.log"

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1" | tee -a "$LOG_FILE"
}

log "Starting backup..."

mkdir -p "$BACKUP_DIR"
chmod 700 "$BACKUP_DIR"

# проверка что контейнер запущен
if ! docker ps --format '{{.Names}}' | grep -q "$CONTAINER"; then
    log "Container $CONTAINER is not running!"
    exit 1
fi

# бэкап
log "Creating backup..."

if docker exec "$CONTAINER" pg_dump -U "$DB_USER" -d "$DB_NAME" > "$BACKUP_DIR/db_$TIMESTAMP.sql" 2>/dev/null; then
    if [ -f "$BACKUP_DIR/db_$TIMESTAMP.sql" ] && [ -s "$BACKUP_DIR/db_$TIMESTAMP.sql" ]; then
        SIZE=$(du -h "$BACKUP_DIR/db_$TIMESTAMP.sql" | cut -f1)
        chmod 600 "$BACKUP_DIR/db_$TIMESTAMP.sql"
        log "Backup created: db_$TIMESTAMP.sql ($SIZE)"
    else
        log "Backup file is empty!"
        rm -f "$BACKUP_DIR/db_$TIMESTAMP.sql"
        exit 1
    fi
else
    log "Backup FAILED!"
    exit 1
fi

# удаление старых бекапов (СТАРШЕ 7 ДНЕЙ) 
log "Cleaning old backups (>$KEEP_DAYS days)..."

DELETED=$(find "$BACKUP_DIR" -name "*.sql" -mtime +$KEEP_DAYS -type f -delete -print 2>/dev/null)
if [ -n "$DELETED" ]; then
    COUNT=$(echo "$DELETED" | wc -l)
    log "   Deleted $COUNT old backup(s)"
fi

# статистика
COUNT=$(ls -1 "$BACKUP_DIR"/*.sql 2>/dev/null | wc -l)
TOTAL_SIZE=$(du -sh "$BACKUP_DIR" 2>/dev/null | cut -f1)
log "Total: $COUNT backup(s), size: $TOTAL_SIZE"
log "Backup completed successfully"
