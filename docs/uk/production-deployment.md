# Інструкція з production-розгортання (Release Engineer / DevOps)

Цей гайд описує розгортання `CarComparisonApi` у production-середовищі.

## 1) Вимоги до апаратного забезпечення

### Підтримувана архітектура
- `x64` Linux сервер (рекомендовано).

### Мінімальні вимоги (невелике середовище)
- CPU: `2 vCPU`
- RAM: `4 GB`
- Диск: `20 GB SSD`

### Рекомендовані вимоги (стабільний production)
- CPU: `4 vCPU`
- RAM: `8 GB`
- Диск: `50+ GB SSD`

## 2) Необхідне ПЗ

На сервері має бути встановлено:
- `.NET 8 Runtime`
- `Nginx` (reverse proxy + TLS termination)
- `systemd`

## 3) Налаштування мережі

Відкрити вхідні порти:
- `80/tcp` (HTTP для redirect/challenge)
- `443/tcp` (HTTPS)
- `22/tcp` (SSH, з обмеженням доступу)

Рекомендовано:
- публічно відкривати тільки `Nginx`;
- API слухає лише localhost (наприклад `127.0.0.1:5060`).

## 4) Конфігурація серверів

### 4.1 Створіть директорії розгортання

```bash
sudo mkdir -p /opt/carcomparison/api
sudo mkdir -p /var/log/carcomparison
sudo chown -R $USER:$USER /opt/carcomparison /var/log/carcomparison
```

### 4.2 Опублікуйте застосунок

З CI або локально:

```bash
dotnet publish CarComparisonApi/CarComparisonApi.csproj -c Release -o ./publish/api
```

Скопіюйте артефакти на сервер у:
- `/opt/carcomparison/api`

### 4.3 Налаштуйте змінні середовища

Секрети задавайте через environment (рекомендовано), наприклад у `systemd`:
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://127.0.0.1:5060`

### 4.4 Створіть systemd сервіс

Приклад: `/etc/systemd/system/carcomparison-api.service`

```ini
[Unit]
Description=CarComparison API
After=network.target

[Service]
WorkingDirectory=/opt/carcomparison/api
ExecStart=/usr/bin/dotnet /opt/carcomparison/api/CarComparisonApi.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=carcomparison-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5060
Environment=Jwt__Key=REPLACE_WITH_SECURE_VALUE
Environment=Jwt__Issuer=CarComparisonApi
Environment=Jwt__Audience=CarComparisonApiUsers

[Install]
WantedBy=multi-user.target
```

Застосуйте сервіс:

```bash
sudo systemctl daemon-reload
sudo systemctl enable carcomparison-api
sudo systemctl start carcomparison-api
sudo systemctl status carcomparison-api
```

### 4.5 Налаштуйте Nginx reverse proxy

Приклад: `/etc/nginx/sites-available/carcomparison`

```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass         http://127.0.0.1:5060;
        proxy_http_version 1.1;
        proxy_set_header   Host $host;
        proxy_set_header   X-Real-IP $remote_addr;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

Увімкніть конфіг:

```bash
sudo ln -s /etc/nginx/sites-available/carcomparison /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

Далі налаштуйте TLS (наприклад Let's Encrypt) і примусовий HTTPS.

## 5) Налаштування СУБД

Поточний стан:
- Реляційна/NoSQL СУБД не використовується.
- Дані зберігаються в JSON-файлах у `CarComparisonApi/Data/`.

Рекомендації для production з поточним підходом:
- тримати JSON-файли на persistent-диску;
- налаштувати коректні права доступу для сервісного користувача;
- включити ці файли в політику резервного копіювання;
- при масштабуванні розглянути міграцію на СУБД.

## 6) Розгортання коду

Рекомендований порядок:
1. Build + test у CI.
2. Публікація артефактів (`dotnet publish -c Release`).
3. Завантаження артефактів на сервер (`/opt/carcomparison/api`).
4. Рестарт сервісу:
   ```bash
   sudo systemctl restart carcomparison-api
   ```
5. Перевірка health та логів.

Стратегія rollback:
- зберігати попередній release;
- повернути попередню директорію/симлінк;
- перезапустити `systemd` сервіс.

## 7) Перевірка працездатності

### Функціональні перевірки

- Тестовий ендпоінт:
  - `GET /api/test` має повертати `200 OK`.
- Swagger/OpenAPI:
  - `GET /swagger/v1/swagger.json` має повертати `200 OK`.
  - `/swagger` має відкриватися.

### Перевірки сервісу

```bash
sudo systemctl status carcomparison-api
sudo journalctl -u carcomparison-api -n 200 --no-pager
```

### Перевірки Nginx

```bash
sudo nginx -t
sudo systemctl status nginx
```

### Критерії успіху

Розгортання вважається успішним, якщо:
- сервіс `carcomparison-api` активний і стабільний;
- API ендпоінти повертають очікувані статус-коди;
- Swagger UI та OpenAPI JSON доступні;
- у логах API/Nginx відсутні критичні помилки.
