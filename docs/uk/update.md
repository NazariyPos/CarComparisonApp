# Оновлення системи (Release Engineer / DevOps)

Цей документ описує чіткий безпечний процес оновлення `CarComparisonApi` у production.

> Поточний стан проєкту: дані зберігаються у JSON (`CarComparisonApi/Data/*.json`), окрема СУБД не використовується.

---

## 1) Підготовка до оновлення

### 1.1 Створення резервних копій

Перед будь-яким оновленням обов'язково створіть бекап:

1. **Поточний реліз коду** (папка застосунку):
```bash
sudo tar -czf /opt/backups/carcomparison/app_$(date +%F_%H%M).tar.gz /opt/carcomparison/api
```

2. **Дані JSON** (критично важливо):
```bash
sudo tar -czf /opt/backups/carcomparison/data_$(date +%F_%H%M).tar.gz /opt/carcomparison/api/Data
```

3. **Конфігурації** (`systemd`, `nginx`):
```bash
sudo tar -czf /opt/backups/carcomparison/config_$(date +%F_%H%M).tar.gz \
  /etc/systemd/system/carcomparison-api.service \
  /etc/nginx/sites-available/carcomparison
```

4. Перевірте, що архіви реально створені:
```bash
ls -lah /opt/backups/carcomparison
```

### 1.2 Перевірка сумісності

Перевірте перед розгортанням:

- версію `.NET Runtime` на сервері:
```bash
dotnet --info
```
- що нова збірка зібрана під `net8.0`;
- сумісність змін у форматі JSON-даних (поля не видалені без міграції);
- сумісність змін у змінних середовища (`Jwt__*`, інші нові параметри).

Рекомендовано зробити smoke-test на staging перед production.

### 1.3 Планування простою

У поточній схемі (1 інстанс + file-based storage) зазвичай потрібен короткий downtime під рестарт сервісу.

Рекомендації:
- заплануйте вікно обслуговування;
- попередьте команду/користувачів;
- підготуйте rollback-команди до початку оновлення.

---

## 2) Процес оновлення

### 2.1 Зупинка потрібних служб

Якщо використовується одиночний інстанс:
```bash
sudo systemctl stop carcomparison-api
sudo systemctl status carcomparison-api
```

`nginx` можна не зупиняти (він віддасть `502`, поки бекенд недоступний), або показати maintenance-сторінку.

### 2.2 Розгортання нового коду

Рекомендований підхід — **релізні папки + симлінк**:

1. Скопіюйте нову збірку:
```bash
sudo mkdir -p /opt/carcomparison/releases/2026-01-15_1200
# copy artifacts into this folder
```

2. Переключіть симлінк `current`:
```bash
sudo ln -sfn /opt/carcomparison/releases/2026-01-15_1200 /opt/carcomparison/current
```

3. Переконайтесь, що `systemd` використовує `current` як `WorkingDirectory/ExecStart`.

> Якщо симлінки не використовуються, оновіть вміст `/opt/carcomparison/api` атомарно (через тимчасову папку + move).

### 2.3 Оновлення конфігурацій

Перевірте:
- `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`;
- `ASPNETCORE_ENVIRONMENT=Production`;
- `ASPNETCORE_URLS=http://127.0.0.1:5060`.

Якщо змінювали `systemd` unit або `nginx` конфігурацію:
```bash
sudo systemctl daemon-reload
sudo nginx -t
sudo systemctl reload nginx
```

### 2.4 Запуск після оновлення

```bash
sudo systemctl start carcomparison-api
sudo systemctl status carcomparison-api
```

Smoke checks:
```bash
curl -i http://127.0.0.1:5060/api/test
curl -i http://127.0.0.1:5060/swagger/v1/swagger.json
```

---

## 3) Процедура rollback (у разі невдалого оновлення)

Rollback потрібно запускати, якщо:
- сервіс не стартує;
- помилки 5xx після релізу;
- критичні функції не працюють;
- зламано формат даних.

### 3.1 Критерії тригера rollback

Вважаємо реліз невдалим, якщо протягом 5–10 хв після деплою:
- `systemctl status` показує `failed/restarting`;
- `/api/test` не дає `200`;
- у `journalctl` є повторювані критичні помилки;
- бізнес-критичні endpoint-и не проходять smoke tests.

### 3.2 Швидкий rollback коду (через симлінк)

1. Зупиніть сервіс:
```bash
sudo systemctl stop carcomparison-api
```

2. Переключіть `current` на попередній реліз:
```bash
sudo ln -sfn /opt/carcomparison/releases/<previous_release> /opt/carcomparison/current
```

3. Запустіть сервіс:
```bash
sudo systemctl start carcomparison-api
sudo systemctl status carcomparison-api
```

4. Перевірте його роботу:
```bash
curl -i http://127.0.0.1:5060/api/test
```

### 3.3 Rollback без симлінків (з backup-архіву)

1. Зупиніть сервіс:
```bash
sudo systemctl stop carcomparison-api
```

2. Відновіть код:
```bash
sudo rm -rf /opt/carcomparison/api
sudo mkdir -p /opt/carcomparison/api
sudo tar -xzf /opt/backups/carcomparison/app_<timestamp>.tar.gz -C /
```

3. За потреби відновіть дані:
```bash
sudo tar -xzf /opt/backups/carcomparison/data_<timestamp>.tar.gz -C /
```

4. Запустіть сервіс та перевірте.

### 3.4 Rollback конфігурації

```bash
sudo tar -xzf /opt/backups/carcomparison/config_<timestamp>.tar.gz -C /
sudo systemctl daemon-reload
sudo nginx -t
sudo systemctl reload nginx
sudo systemctl restart carcomparison-api
```

### 3.5 Post-rollback дії (обов'язково)

1. Зафіксуйте інцидент (час, версія релізу, симптоми).
2. Збережіть логи:
```bash
sudo journalctl -u carcomparison-api -n 500 --no-pager > /tmp/carcomparison_rollback.log
```
3. Позначте реліз як failed у changelog/трекері.
4. Призначте root-cause analysis і план повторного релізу.

