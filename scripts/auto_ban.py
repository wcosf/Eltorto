import re
import time
import subprocess
import os
from collections import defaultdict

LOG_PATH = "/root/eltorto.ru/infra/nginx/logs/access.log"
BAN_DURATION = 3600
REQUEST_LIMIT = 500
BAN_LIST = "/root/eltorto.ru/infra/nginx/ban.conf"


SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
WHITELIST_FILE = os.path.join(SCRIPT_DIR, "whitelist.txt")
PATTERNS_FILE = os.path.join(SCRIPT_DIR, "spam_patterns.txt")

whitelist = []
spam_patterns = []
banned_ips = {}
last_position = 0

def load_whitelist():
    global whitelist
    whitelist = []
    try:
        with open(WHITELIST_FILE, 'r') as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#'):
                    whitelist.append(line)
        print(f"Loaded {len(whitelist)} IPs to whitelist")
    except FileNotFoundError:
        print(f"Warning: {WHITELIST_FILE} not found")

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

def update_ban_list():
    current_time = time.time()
    active_bans = {ip: until for ip, until in banned_ips.items() if until > current_time}
    banned_ips.clear()
    banned_ips.update(active_bans)
    
    try:
        with open(BAN_LIST, 'w') as f:
            f.write("# Auto-generated ban list\n")
            for ip in active_bans:
                f.write(f"deny {ip};\n")
        
        subprocess.run(["nginx", "-s", "reload"], capture_output=True)
    except Exception as e:
        print(f"Error updating ban list: {e}")

def main():
    load_whitelist()
    load_spam_patterns()
    
    ip_requests = defaultdict(int)
    ip_spam = defaultdict(int)
    
    for line in get_log_lines():
        match = re.search(r'^(\d+\.\d+\.\d+\.\d+)', line)
        if match:
            ip = match.group(1)
            
            if ip in whitelist:
                continue
            if ip.startswith(('127.', '172.', '192.168.', '10.')):
                continue

            ip_requests[ip] += 1

            if is_spam_comment(line):
                ip_spam[ip] += 1
                print(f"Spam detected from {ip}")
    
    # Бан по количеству запросов
    for ip, count in ip_requests.items():
        if count > REQUEST_LIMIT and ip not in banned_ips:
            banned_ips[ip] = time.time() + BAN_DURATION
            print(f"!!! Banned {ip} for {BAN_DURATION//60} minutes ({count} requests)")
    
    # Бан за спам 
    for ip, count in ip_spam.items():
        if ip not in banned_ips:
            banned_ips[ip] = time.time() + BAN_DURATION
            print(f"!!! Banned {ip} for spam ({count} spam comments)")
    
    update_ban_list()

if __name__ == "__main__":
    main()