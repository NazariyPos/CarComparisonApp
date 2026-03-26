# Резервне копіювання CarComparisonApi (Release Engineer / DevOps)

Цей гайд описує практичну стратегію backup/restore для поточної архітектури проєкту:
- API: `CarComparisonApi` (.NET 8)
- Сховище даних: JSON-файли у `CarComparisonApi/Data/*.json`
- Типове production-розгортання: Linux + `systemd` + `Nginx`

## 1) Стратегія резервного копіювання

### 1.1 Типи резервних копій

Рекомендовано комбінувати 3 типи:

1. **Повний backup (Full)**
   - Копія всіх критичних даних і конфігурацій.
   - Найпростіший для відновлення.

2. **Інкрементальний backup (Incremental)**
   - Копіює лише зміни від попереднього backup (full або incremental).
   - Економить місце, але відновлення довше (потрібен ланцюжок копій).

3. **Диференціальний backup (Differential)**
   - Копіює зміни від останнього full backup.
   - Швидше відновлення, ніж incremental, але займає більше місця.

### 1.2 Рекомендована частота

Для невеликого/середнього навантаження:

- **Full**: щонеділі о `02:00`
- **Differential**: щодня о `02:00` (пн-сб)
- **Incremental**: кожні `4` години

Якщо дані змінюються рідко, можна спростити:
- Full раз на тиждень
- Incremental 1-2 рази на день

### 1.3 Зберігання та ротація

Застосовуйте правило **3-2-1**:
- `3` копії даних
- `2` різні носії
- `1` копія поза основним сервером (off-site)

Приклад ротації:
- Щоденні копії: зберігати `14` днів
- Щотижневі: `8` тижнів
- Щомісячні: `12` місяців

Рекомендовано:
- шифрувати backups (`restic`, `borg`, або шифрований S3 bucket);
- зберігати checksum/manifest для перевірки цілісності;
- обмежити доступ до backup-сховища (least privilege).

---

## 2) Процедура резервного копіювання

Нижче наведено обов’язкові об’єкти для backup.

### 2.1 Дані застосунку (замість БД у поточній архітектурі)

У проєкті наразі немає SQL/NoSQL бази. Роль «даних БД» виконують файли:
- `CarComparisonApi/Data/cars.json`
- `CarComparisonApi/Data/users.json`
- інші `*.json` у цій директорії

Що робити:
1. Короткочасно «заморозити» запис (або виконувати backup у вікно низького навантаження).
2. Зробити архів директорії `CarComparisonApi/Data/`.
3. Порахувати checksum (`sha256`).
4. Відправити архів у backup-сховище.

### 2.2 Файли конфігурації

Критично важливі для відновлення:
- `CarComparisonApi/appsettings.json`
- `CarComparisonApi/appsettings.Production.json` (якщо використовується)
- `CarComparisonApi/Properties/launchSettings.json` (для dev/lab)
- `systemd` unit: `/etc/systemd/system/carcomparison-api.service`
- `Nginx` конфіг: `/etc/nginx/sites-available/carcomparison`

**Секрети** (JWT ключі) не зберігати у відкритому вигляді в Git.
Рекомендовано:
- backup secure secret-store (Vault/KeyVault/SSM), або
- зашифрований `.env`/secret-файл у захищеному сховищі.

### 2.3 Користувацькі дані

У поточній реалізації користувацькі дані також містяться у JSON файлах (`users.json`, частково `cars.json`/reviews/favorites залежно від моделі).

Політика:
- backup разом із `Data/`;
- версіонування backup-архівів за timestamp (`YYYYMMDD-HHMM`);
- зберігати метадані: розмір, checksum, дата, версія застосунку.

### 2.4 Логи системи

Мінімальний набір для інцидентів і аудиту:
- `journalctl` логи сервісу `carcomparison-api`
- `Nginx` access/error логи (`/var/log/nginx/*`)
- (опційно) application logs, якщо виводяться у файли

Політика:
- щоденний експорт логів у архів;
- термін зберігання логів: мінімум `30-90` днів (залежить від вимог).

---

## 3) Перевірка цілісності резервних копій

Після кожного backup-циклу:

1. **Перевірка checksum**
   - обчислити `sha256` для архіву;
   - порівняти з manifest-файлом.

2. **Перевірка читабельності архіву**
   - тестове розпакування у тимчасову директорію;
   - перевірка наявності критичних файлів.

3. **Логічна перевірка даних**
   - JSON parse/валідація для `Data/*.json`.

4. **Періодичний test-restore**
   - мінімум 1 раз на місяць підіймати тестовий стенд з backup.

---

## 4) Автоматизація процесу резервного копіювання

Найпростіший production-підхід: `bash` + `cron` + `restic`.

### 4.1 Приклад bash-скрипта (Linux)

```bash
#!/usr/bin/env bash
set -euo pipefail

TS="$(date +%Y%m%d-%H%M%S)"
SRC_APP="/opt/carcomparison/api"
SRC_DATA="$SRC_APP/Data"
SRC_CFG1="/etc/systemd/system/carcomparison-api.service"
SRC_CFG2="/etc/nginx/sites-available/carcomparison"
OUT_DIR="/opt/backups/carcomparison"
ARCHIVE="$OUT_DIR/carcomparison-$TS.tar.gz"
MANIFEST="$ARCHIVE.sha256"

mkdir -p "$OUT_DIR"

# Архів критичних даних + конфігів
tar -czf "$ARCHIVE" \
  -C "$SRC_APP" Data \
  "$SRC_CFG1" \
  "$SRC_CFG2"

sha256sum "$ARCHIVE" > "$MANIFEST"

# За потреби: відправити у S3/Blob/restic repo
# restic -r s3:s3.amazonaws.com/my-backups backup "$ARCHIVE" "$MANIFEST"

echo "Backup created: $ARCHIVE"
```

### 4.2 Планувальник (cron)

Приклад:

```cron
0 */4 * * * /usr/local/bin/carcomparison-backup.sh >> /var/log/carcomparison-backup.log 2>&1
```

### 4.3 Рекомендовані інструменти

- `restic` — шифрування, дедуплікація, ротація
- `borg` — ефективні backup snapshots
- `rclone` — відправка в хмарні сховища
- Cloud-native: AWS Backup / Azure Backup / GCP Backup (за інфраструктурою)

---

## 5) Процедура відновлення з резервних копій

## 5.1 Повне відновлення системи

Сценарій: повна втрата сервера або критичне пошкодження.

Кроки:
1. Підготувати новий сервер (OS, .NET runtime, Nginx, systemd).
2. Зупинити API сервіс:
   ```bash
   sudo systemctl stop carcomparison-api
   ```
3. Відновити артефакти застосунку у `/opt/carcomparison/api`.
4. Відновити `Data/` з backup-архіву.
5. Відновити `systemd` unit і Nginx конфіг.
6. Виконати `systemctl daemon-reload`, стартувати сервіси.
7. Перевірити працездатність (`/api/test`, `/swagger`).

## 5.2 Вибіркове відновлення даних

Сценарій: пошкоджено лише частину файлів (наприклад `users.json`).

Кроки:
1. Зупинити сервіс (або перевести у read-only режим).
2. Розпакувати потрібний backup у тимчасову директорію.
3. Замінити лише потрібні файли (`users.json` тощо).
4. Перевірити JSON-валідність.
5. Запустити сервіс та перевірити функціональність.

## 5.3 Тестування відновлення

Обов’язкова практика:
- Щомісяця виконувати test-restore на окремому середовищі.
- Фіксувати RTO/RPO:
  - **RTO** — за який час система відновилась;
  - **RPO** — яку максимальну втрату даних прийнято.
- Оформлювати короткий звіт: дата, backup-версія, результат, проблеми.

---

## Чеклист для Release Engineer / DevOps

- [ ] Впроваджено правило 3-2-1
- [ ] Налаштовано розклад full/incremental/differential
- [ ] Додається backup `Data/`, конфігів, логів
- [ ] Налаштована ротація та шифрування
- [ ] Є checksum і перевірка архівів
- [ ] Є регулярний test-restore
- [ ] Документовано RTO/RPO та відповідальних
