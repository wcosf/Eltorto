FROM node:18-alpine AS builder

WORKDIR /app

# Копируем package.json и устанавливаем зависимости
COPY package*.json ./
RUN npm ci --legacy-peer-deps

COPY . .

# Собираем приложение
RUN npm run build -- --configuration=production

# Stage 2: Serve with nginx
FROM nginx:alpine

# Копируем собранное приложение из папки browser
COPY --from=builder /app/dist/frontend/browser /usr/share/nginx/html

# Проверяем что скопировалось
RUN echo "Files in /usr/share/nginx/html:" && ls -la /usr/share/nginx/html/

COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
