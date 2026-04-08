import os
import time
from PIL import Image
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler

WATCH_DIR = "/var/www/uploads"
MAX_WIDTH = 1200
QUALITY = 85

class ImageHandler(FileSystemEventHandler):
    def on_created(self, event):
        if event.is_directory:
            return
        if event.src_path.lower().endswith(('.png', '.jpg', '.jpeg')):
            time.sleep(1)
            optimize_image(event.src_path)

def optimize_image(input_path):
    try:
        output_path = input_path.rsplit('.', 1)[0] + '.webp'
        with Image.open(input_path) as img:
            if img.width > MAX_WIDTH:
                ratio = MAX_WIDTH / img.width
                new_height = int(img.height * ratio)
                img = img.resize((MAX_WIDTH, new_height), Image.Resampling.LANCZOS)
            if img.mode in ('RGBA', 'LA', 'P'):
                bg = Image.new('RGB', img.size, (255, 255, 255))
                bg.paste(img, mask=img.split()[-1] if img.mode == 'RGBA' else None)
                img = bg
            img.save(output_path, 'WEBP', quality=QUALITY, optimize=True)
        os.remove(input_path)
    except Exception as e:
        pass

if __name__ == "__main__":
    os.makedirs(WATCH_DIR, exist_ok=True)
    observer = Observer()
    observer.schedule(ImageHandler(), WATCH_DIR, recursive=False)
    observer.start()
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        observer.stop()
    observer.join()