#!/bin/bash

LOG_FILE="/var/log/backup_db.log"
MAX_SIZE_MB=10

if [ -f "$LOG_FILE" ]; then
    SIZE=$(du -m "$LOG_FILE" | cut -f1)
    if [ $SIZE -gt $MAX_SIZE_MB ]; then
        echo "[$(date)] Rotating backup log (size: ${SIZE}MB)"
        > "$LOG_FILE"
        echo "[$(date)] Log rotated" > "$LOG_FILE"
    fi
fi