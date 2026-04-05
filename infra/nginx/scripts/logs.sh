#!/bin/sh

LOG_BASE_DIR="/var/log/nginx"
CURRENT_DATE=$(date +%Y_%m_%d)
LOG_DIR="$LOG_BASE_DIR/$CURRENT_DATE"
ACCESS_LOG="$LOG_DIR/access_$CURRENT_DATE.log"
ERROR_LOG="$LOG_DIR/error_$CURRENT_DATE.log"

RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
CYAN='\033[0;36m'
NC='\033[0m'  # No Color

colorize_line() {
    local line="$1"
    if [[ "$line" == *"[error]"* ]]; then
        echo -e "${line/\[error\]/\[${RED}error${NC}\]}"
    elif [[ "$line" == *"[warn]"* ]]; then
        echo -e "${line/\[warn\]/\[${YELLOW}warn${NC}\]}"
    elif [[ "$line" == *"[notice]"* ]]; then
        echo -e "${line/\[notice\]/\[${CYAN}notice${NC}\]}"
    else
        if echo "$line" | grep -qE '" 200 '; then
            echo -e "${line/ 200 / ${GREEN}200${NC} }"
        elif echo "$line" | grep -qE '" 301 '; then
            echo -e "${line/ 301 / ${CYAN}301${NC} }"
        elif echo "$line" | grep -qE '" 304 '; then
            echo -e "${line/ 304 / ${CYAN}304${NC} }"
        elif echo "$line" | grep -qE '" 404 '; then
            echo -e "${line/ 404 / ${CYAN}404${NC} }"
        elif echo "$line" | grep -qE '" 500 '; then
            echo -e "${line/ 500 / ${RED}500${NC} }"
        else
            echo "$line"
        fi
    fi
}

mkdir -p "$LOG_DIR"
echo "Создана папка: $LOG_DIR"

if [ -f "$LOG_BASE_DIR/access.log" ]; then
    if [ -s "$LOG_BASE_DIR/access.log" ]; then
        echo "Обработка access.log..."
        while IFS= read -r line; do
            colorize_line "$line" >> "$ACCESS_LOG"
        done < "$LOG_BASE_DIR/access.log"
    fi
    rm -f "$LOG_BASE_DIR/access.log"
    echo "access.log обработан и удален"
fi

if [ -f "$LOG_BASE_DIR/error.log" ]; then
    if [ -s "$LOG_BASE_DIR/error.log" ]; then
        echo "Обработка error.log..."
        while IFS= read -r line; do
            colorize_line "$line" >> "$ERROR_LOG"
        done < "$LOG_BASE_DIR/error.log"
    fi
    rm -f "$LOG_BASE_DIR/error.log"
    echo "error.log обработан и удален"
fi

nginx -s reopen 2>/dev/null || kill -USR1 $(cat /var/run/nginx.pid) 2>/dev/null

echo "Сбор логов завершен: $CURRENT_DATE"
echo "Файлы созданы в: $LOG_DIR"
ls -la "$LOG_DIR"