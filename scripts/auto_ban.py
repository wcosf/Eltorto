import re
import time
import subprocess
import os
from collections import defaultdict

LOG_PATH = "/root/eltorto.ru/infra/nginx/logs/access.log"
BAN_DURATION = 10**10  
REQUEST_LIMIT = 500     
ERROR_LIMIT = 20
BAN_LIST = "/root/eltorto.ru/infra/nginx/ban.conf"
MANUAL_BAN_LIST = "/root/eltorto.ru/infra/nginx/ban_manual.conf"

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
WHITELIST_FILE = os.path.join(SCRIPT_DIR, "whitelist.txt")
PATTERNS_FILE = os.path.join(SCRIPT_DIR, "spam_patterns.txt")

whitelist = []
spam_patterns = []
banned_ips = {}
last_position = 0

SUSPICIOUS_USER_AGENTS = [
    'python-requests',
    'Python-urllib',
    'curl',
    'wget',
    'scanner',
    'spider',
    'crawler',
    'bot',
    'scan',
    'nmap',
    'masscan',
    'zgrab',
    'http-client',
    'libwww',
    'perl',
    'lwp-request',
    'axios',
    'node-fetch',
    'go-http-client',
    'java',
    'okhttp'
]

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

def is_suspicious_user_agent(line):
    for agent in SUSPICIOUS_USER_AGENTS:
        if agent.lower() in line.lower():
            return True
    return False

def is_scanner(line):
    if '404' in line:
        scanner_paths = [
            '/wp-', '/wordpress', '/admin', '/phpmyadmin',
            '/xmlrpc', '/.env', '/.git', '/config',
            '/backup', '/.aws', '/credentials',
            '/shell', '/cmd', '/exec', '/cgi-bin',
            '/vendor', '/composer', '/package',
            '/sql', '/mysql', '/database',
            '/.ssh', '/id_rsa', '/.bash_history',
            '/webconfig', '/.htaccess', '/.htpasswd',
            '/tmp', '/temp', '/cache',
            '/test', '/demo', '/sample',
            '/api/v1', '/api/v2', '/v1', '/v2',
            '/swagger', '/openapi', '/docs',
            '/grafana', '/prometheus', '/metrics',
            '/actuator', '/health', '/env',
            '/wp-login', '/wp-admin', '/wp-content'
        ]
        for path in scanner_paths:
            if path in line:
                return True
    return False

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
            f.write("# === РУЧНОЙ БАН ===\n")
            f.write("# Добавляйте сюда IP в формате: deny IP;\n")
    return manual_bans

def update_ban_list():
    current_time = time.time()
    active_bans = dict(banned_ips) 
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
            f.write("# Бан за спам, сканирование, подозрительные User-Agent\n")
            for ip in active_bans:
                f.write(f"deny {ip};\n")
        
        subprocess.run(["docker", "exec", "eltorto_nginx", "nginx", "-s", "reload"], capture_output=True)
        print(f"Ban list updated: {len(active_bans)} auto-bans, {len(manual_bans)} manual bans")
    except Exception as e:
        print(f"Error updating ban list: {e}")

def main():
    load_whitelist()
    load_spam_patterns()
    
    import socket
    try:
        server_ip = socket.gethostbyname(socket.gethostname())
        print(f"Server IP detected: {server_ip}")
    except:
        server_ip = None
        print("Could not detect server IP")
    
    ip_requests = defaultdict(int)
    ip_spam = defaultdict(int)
    ip_errors = defaultdict(int)
    ip_scanner = defaultdict(int)
    ip_suspicious_ua = defaultdict(int)
    
    for line in get_log_lines():
        match = re.search(r'^(\d+\.\d+\.\d+\.\d+)', line)
        if match:
            ip = match.group(1)
            
            if ip in whitelist:
                continue
            if ip.startswith(('127.', '10.', '192.168.')):
                continue
            if server_ip and ip == server_ip:
                continue

            ip_requests[ip] += 1

            if is_spam_comment(line):
                ip_spam[ip] += 1
                print(f"Spam detected from {ip}")

            if is_scanner(line):
                ip_scanner[ip] += 1
                print(f"Scanner detected from {ip} (404 on suspicious path)")

            if is_suspicious_user_agent(line):
                ip_suspicious_ua[ip] += 1
                print(f"Suspicious User-Agent from {ip}")

            if '404' in line:
                ip_errors[ip] += 1

    for ip, count in ip_spam.items():
        if ip not in banned_ips:
            banned_ips[ip] = time.time() + BAN_DURATION
            print(f"!!! BANNED {ip} for spam ({count} spam comments)")

    for ip, count in ip_requests.items():
        if count > REQUEST_LIMIT and ip not in banned_ips:
            banned_ips[ip] = time.time() + BAN_DURATION
            print(f"!!! BANNED {ip} for {BAN_DURATION//60} minutes ({count} requests)")

    for ip, count in ip_errors.items():
        if count > ERROR_LIMIT and ip not in banned_ips:
            banned_ips[ip] = time.time() + BAN_DURATION
            print(f"!!! BANNED {ip} for scanning ({count} 404 errors)")

    for ip, count in ip_scanner.items():
        if ip not in banned_ips:
            banned_ips[ip] = time.time() + BAN_DURATION
            print(f"!!! BANNED {ip} for path scanning ({count} suspicious paths)")

    for ip, count in ip_suspicious_ua.items():
        if ip not in banned_ips:
            banned_ips[ip] = time.time() + BAN_DURATION
            print(f"!!! BANNED {ip} for suspicious User-Agent ({count} requests)")

    update_ban_list()

if __name__ == "__main__":
    main()