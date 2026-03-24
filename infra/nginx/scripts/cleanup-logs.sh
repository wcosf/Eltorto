#!/bin/sh

# Скрипт очистки старых логов (удаляет папки старше 7 дней)

LOG_BASE_DIR="/var/log/nginx"
DAYS_TO_KEEP=7

echo "Очистка логов:"
echo "Запуск: $(date)"

# нормализация дат
date_to_seconds() {
    local date_str="$1"
    local year=$(echo "$date_str" | cut -d'_' -f1)
    local month=$(echo "$date_str" | cut -d'_' -f2)
    local day=$(echo "$date_str" | cut -d'_' -f3)
    date -d "$year-$month-$day" +%s 2>/dev/null
}

CURRENT_TIMESTAMP=$(date +%s)

for dir in "$LOG_BASE_DIR"/*/; do
    dir_name=$(basename "$dir")
    if [[ $dir_name =~ ^[0-9]{4}_[0-9]{2}_[0-9]{2}$ ]]; then
        dir_timestamp=$(date_to_seconds "$dir_name")
        
        if [ -n "$dir_timestamp" ]; then
            if [ $CURRENT_TIMESTAMP -lt $dir_timestamp ]; then
                days_diff=0
            else
                days_diff=$(( (CURRENT_TIMESTAMP - dir_timestamp) / 86400 ))
            fi

            if [ $days_diff -gt $DAYS_TO_KEEP ]; then
                echo "Удаляем папку: $dir (возраст: $days_diff дней)"
                rm -rf "$dir"
            fi
        fi
    fi
done

echo "Очистка завершена: $(date)"