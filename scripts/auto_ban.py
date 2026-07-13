import re
import time
import subprocess
import os
from collections import defaultdict

LOG_PATH = "/root/eltorto.ru/infra/nginx/logs/access.log"
BAN_DURATION = 3600  # 1 час 
BAN_LIST = "/root/eltorto.ru/infra/nginx/ban.conf"
MANUAL_BAN_LIST = "/root/eltorto.ru/infra/nginx/ban_manual.conf"

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PATTERNS_FILE = os.path.join(SCRIPT_DIR, "spam_patterns.txt")

spam_patterns = []
banned_ips = {}
last_position = 0

def load_spam_patterns():
    global spam_patterns
    spam_patterns = []
    try:
        with open(PATTERNS_FILE, 'r') as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#'):
                    spam_patterns.append(line)
        print(f"Loaded {len(spam_patterns)} spam patterns")
    except FileNotFoundError:
        print(f"Warning: {PATTERNS_FILE} not found")

def get_log_lines():
    global last_position
    try:
        with open(LOG_PATH, 'r') as f:
            f.seek(last_position)
            lines = f.readlines()
            last_position = f.tell()
        return lines
    except FileNotFoundError:
        print(f"Warning: {LOG_PATH} not found")
        return []

def is_spam_comment(line):
    if 'POST /api/testimonials' in line or 'POST /api/Testimonials' in line:
        for pattern in spam_patterns:
            if re.search(pattern, line, re.IGNORECASE):
                return True
    return False

def load_manual_bans():
    manual_bans = []
    try:
        with open(MANUAL_BAN_LIST, 'r') as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#'):
                    manual_bans.append(line)
        print(f"Loaded {len(manual_bans)} manual bans")
    except FileNotFoundError:
        print(f"Warning: {MANUAL_BAN_LIST} not found, creating...")
        with open(MANUAL_BAN_LIST, 'w') as f:
            f.write("#------ РУЧНОЙ БАН ------\n")
            f.write("# IP в формате: deny IP;\n")
    return manual_bans

def update_ban_list():
    current_time = time.time()
    # Удаляем истекшие баны
    active_bans = {ip: until for ip, until in banned_ips.items() if until > current_time}
    banned_ips.clear()
    banned_ips.update(active_bans)
    
    try:
        manual_bans = load_manual_bans()
        
        with open(BAN_LIST, 'w') as f:
            f.write("# Auto-generated ban list\n")
            f.write("#РУЧНОЙ БАН \n")
            for ban in manual_bans:
                f.write(f"{ban}\n")
            f.write("\n#------АВТОМАТИЧЕСКИЙ БАН------\n")
            f.write("# Бан только за спам в отзывах\n")
            for ip in active_bans:
                f.write(f"deny {ip};\n")
        
        subprocess.run(["docker", "exec", "eltorto_nginx", "nginx", "-s", "reload"], capture_output=True)
        print(f"Ban list updated: {len(active_bans)} auto-bans, {len(manual_bans)} manual bans")
    except Exception as e:
        print(f"Error updating ban list: {e}")

def main():
    load_spam_patterns()
    
    ip_spam = defaultdict(int)
    
    for line in get_log_lines():
        match = re.search(r'^(\d+\.\d+\.\d+\.\d+)', line)
        if match:
            ip = match.group(1)
            
            # Пропускаем локальные IP
            if ip.startswith(('127.', '10.', '192.168.')):
                continue

            if is_spam_comment(line):
                ip_spam[ip] += 1
                print(f"Spam detected from {ip}")

    # Бан за спам
    for ip, count in ip_spam.items():
        if ip not in banned_ips:
            banned_ips[ip] = time.time() + BAN_DURATION
            print(f"!!! BANNED {ip} for spam ({count} spam comments)")

    update_ban_list()

if __name__ == "__main__":
    main()