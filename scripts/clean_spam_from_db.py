#!/usr/bin/env python3
import re
import psycopg2
import os
from datetime import datetime

DB_NAME = os.getenv("DB_NAME", "eltorto_pg")
DB_USER = os.getenv("DB_USER", "postgres")
DB_PASSWORD = os.getenv("DB_PASSWORD") 
DB_HOST = os.getenv("DB_HOST", "localhost")
DB_PORT = os.getenv("DB_PORT", "5432")

# Путь к файлу со спам-паттернами
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PATTERNS_FILE = os.path.join(SCRIPT_DIR, "spam_patterns.txt")

# Логирование
LOG_FILE = "/var/log/spam_cleaner.log"

def log(message):
    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    with open(LOG_FILE, 'a') as f:
        f.write(f"[{timestamp}] {message}\n")
    print(f"[{timestamp}] {message}")

def load_spam_patterns():
    patterns = []
    try:
        with open(PATTERNS_FILE, 'r') as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#'):
                    patterns.append(line)
        log(f"Loaded {len(patterns)} spam patterns")
        return patterns
    except FileNotFoundError:
        log(f"ERROR: {PATTERNS_FILE} not found")
        return []

def get_db_connection():
    if not DB_PASSWORD:
        log("ERROR: DB_PASSWORD environment variable not set!")
        return None
    try:
        conn = psycopg2.connect(
            dbname=DB_NAME,
            user=DB_USER,
            password=DB_PASSWORD,
            host=DB_HOST,
            port=DB_PORT
        )
        return conn
    except Exception as e:
        log(f"ERROR: Cannot connect to database: {e}")
        return None

def delete_spam_testimonials(patterns):
    conn = get_db_connection()
    if not conn:
        return 0
    
    cursor = conn.cursor()
    deleted_count = 0

    try:
        cursor.execute('''
            SELECT "Id", "Author", "Text", "Email" 
            FROM public."Testimonials" 
            WHERE "IsApproved" = false
        ''')
        testimonials = cursor.fetchall()

        for row in testimonials:
            id, author, text, email = row
            full_text = f"{author} {text} {email if email else ''}"
            
            for pattern in patterns:
                if re.search(pattern, full_text, re.IGNORECASE):
                    cursor.execute('DELETE FROM public."Testimonials" WHERE "Id" = %s', (id,))
                    deleted_count += 1
                    log(f"DELETED spam testimonial ID={id} from '{author}'")
                    break

        conn.commit()
        log(f"Total deleted: {deleted_count} spam testimonials")

    except Exception as e:
        log(f"ERROR: {e}")
        conn.rollback()
    finally:
        cursor.close()
        conn.close()

    return deleted_count

def main():
    log("Начало Очистки:")
    patterns = load_spam_patterns()
    if patterns:
        delete_spam_testimonials(patterns)
    log("Спам очищен")

if __name__ == "__main__":
    main()