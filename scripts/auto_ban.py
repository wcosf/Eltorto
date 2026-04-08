import re
import time
import subprocess
from collections import defaultdict

LOG_PATH = "/var/log/nginx/access.log"
BAN_DURATION = 3600
REQUEST_LIMIT = 200
BAN_LIST = "/etc/nginx/ban.conf"

banned_ips = {}
last_position = 0

def get_log_lines():
    global last_position
    with open(LOG_PATH, 'r') as f:
        f.seek(last_position)
        lines = f.readlines()
        last_position = f.tell()
    return lines

def update_ban_list():
    current_time = time.time()
    active_bans = {ip: until for ip, until in banned_ips.items() if until > current_time}
    banned_ips.clear()
    banned_ips.update(active_bans)
    
    with open(BAN_LIST, 'w') as f:
        f.write("# Auto-generated ban list\n")
        for ip in active_bans:
            f.write(f"deny {ip};\n")
    
    subprocess.run(["nginx", "-s", "reload"], capture_output=True)

def main():
    ip_requests = defaultdict(int)
    
    for line in get_log_lines():
        match = re.search(r'^(\d+\.\d+\.\d+\.\d+)', line)
        if match:
            ip = match.group(1)
            if not ip.startswith(('127.', '172.', '192.168.', '10.')):
                ip_requests[ip] += 1
    
    for ip, count in ip_requests.items():
        if count > REQUEST_LIMIT and ip not in banned_ips:
            banned_ips[ip] = time.time() + BAN_DURATION
            print(f"Banned {ip} for {BAN_DURATION//60} minutes ({count} requests)")
    
    update_ban_list()

if __name__ == "__main__":
    main()