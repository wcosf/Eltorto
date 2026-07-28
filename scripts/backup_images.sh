#!/bin/bash

# -------------------------------------------------------------
# БЭКАП ФОТОГРАФИЙ ELTORTO
# Место хранения: /var/backups/eltorto/
# Хранение: 7 дней (старые удаляются)
# Логи: /var/log/backup_images.log

BACKUP_DIR="/var/backups/eltorto"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
KEEP_DAYS=7
LOG_FILE="/var/log/backup_images.log"

PHOTO_PATHS=(
    "/root/eltorto.ru/frontend/src/assets/images"
    "/root/eltorto.ru/frontend/src/assets/icons"
    "/root/eltorto.ru/frontend/src/assets/backgrounds"
)

log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1" | tee -a "$LOG_FILE"
}

log "Starting images backup..."

mkdir -p "$BACKUP_DIR"
chmod 700 "$BACKUP_DIR"

# проверка наличия папки
FOUND=0
for path in "${PHOTO_PATHS[@]}"; do
    if [ -d "$path" ]; then
        FILE_COUNT=$(find "$path" -type f 2>/dev/null | wc -l)
        if [ "$FILE_COUNT" -gt 0 ]; then
            FOUND=1
            log "Found folder: $path ($FILE_COUNT files)"
        fi
    else
        log "Folder not found: $path"
    fi
done

if [ "$FOUND" -eq 0 ]; then
    log "No image folders found! Check PHOTO_PATHS in script."
    exit 1
fi

# архив с фото
BACKUP_FILE="$BACKUP_DIR/images_$TIMESTAMP.tar.gz"
log "Creating archive: $BACKUP_FILE"

tar -czf "$BACKUP_FILE" "${PHOTO_PATHS[@]}" 2>/dev/null

# проверка, что архив создался
if [ -f "$BACKUP_FILE" ] && [ -s "$BACKUP_FILE" ]; then
    SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
    chmod 600 "$BACKUP_FILE"
    log "Backup created: images_$TIMESTAMP.tar.gz ($SIZE)"
else
    log "Backup file is empty or not created!"
    rm -f "$BACKUP_FILE"
    exit 1
fi

# удаление старых бекапов
log "Cleaning old backups (>$KEEP_DAYS days)..."

DELETED=$(find "$BACKUP_DIR" -name "images_*.tar.gz" -mtime +$KEEP_DAYS -type f -delete -print 2>/dev/null)
if [ -n "$DELETED" ]; then
    COUNT=$(echo "$DELETED" | wc -l)
    log "   Deleted $COUNT old backup(s)"
fi

# статистика тестовое
COUNT=$(ls -1 "$BACKUP_DIR"/images_*.tar.gz 2>/dev/null | wc -l)
TOTAL_SIZE=$(du -sh "$BACKUP_DIR" 2>/dev/null | cut -f1)
log "Total: $COUNT image backup(s), size: $TOTAL_SIZE"
log "Images backup completed successfully"