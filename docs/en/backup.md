# CarComparisonApi Backup Guide (Release Engineer / DevOps)

This guide provides a practical backup/restore strategy for the current project architecture:
- API: `CarComparisonApi` (.NET 8)
- Data store: JSON files in `CarComparisonApi/Data/*.json`
- Typical production stack: Linux + `systemd` + `Nginx`

## 1) Backup strategy

### 1.1 Backup types

Use a combination of three types:

1. **Full backup**
   - A complete copy of all critical data and configuration.
   - Easiest restore path.

2. **Incremental backup**
   - Stores only changes since the previous backup (full or incremental).
   - Saves storage, but restore requires a backup chain.

3. **Differential backup**
   - Stores changes since the last full backup.
   - Faster restore than incremental, larger storage usage.

### 1.2 Recommended frequency

For small/medium workload:

- **Full**: every Sunday at `02:00`
- **Differential**: daily at `02:00` (Mon-Sat)
- **Incremental**: every `4` hours

If data changes rarely, simplify to:
- weekly full
- 1-2 incremental backups per day

### 1.3 Storage and retention

Apply the **3-2-1** rule:
- `3` copies of data
- `2` different media
- `1` off-site copy

Example retention:
- Daily backups: keep `14` days
- Weekly backups: keep `8` weeks
- Monthly backups: keep `12` months

Recommended:
- encrypt backups (`restic`, `borg`, encrypted S3 bucket);
- store checksums/manifests for integrity checks;
- restrict backup storage access (least privilege).

---

## 2) Backup procedure

### 2.1 Application data (database-equivalent in current architecture)

There is no SQL/NoSQL database in the current implementation. Data files are:
- `CarComparisonApi/Data/cars.json`
- `CarComparisonApi/Data/users.json`
- other `*.json` in this directory

Procedure:
1. Briefly freeze writes (or run during low-traffic window).
2. Archive `CarComparisonApi/Data/`.
3. Calculate checksum (`sha256`).
4. Upload archive to backup storage.

### 2.2 Configuration files

Critical for restore:
- `CarComparisonApi/appsettings.json`
- `CarComparisonApi/appsettings.Production.json` (if used)
- `CarComparisonApi/Properties/launchSettings.json` (dev/lab)
- `systemd` unit: `/etc/systemd/system/carcomparison-api.service`
- `Nginx` config: `/etc/nginx/sites-available/carcomparison`

Do not store secrets (JWT keys) in plain text in Git.
Recommended:
- backup secure secret store (Vault/KeyVault/SSM), or
- keep encrypted secret file in protected storage.

### 2.3 User data

In the current implementation, user data is also in JSON files (`users.json`, and potentially reviews/favorites in related JSON models).

Policy:
- backup together with `Data/`;
- version backup archives with timestamp (`YYYYMMDD-HHMM`);
- keep metadata: size, checksum, date, app version.

### 2.4 System logs

Minimum set for incident analysis and audit:
- `journalctl` logs for `carcomparison-api`
- `Nginx` access/error logs (`/var/log/nginx/*`)
- optional application file logs (if enabled)

Policy:
- daily log export to archive;
- retain logs at least `30-90` days (by compliance requirements).

---

## 3) Backup integrity verification

After each backup cycle:

1. **Checksum validation**
   - compute `sha256` for backup archive;
   - compare with manifest.

2. **Archive readability check**
   - test extract to temporary folder;
   - verify critical files exist.

3. **Logical data validation**
   - parse/validate JSON files in `Data/*.json`.

4. **Periodic test restore**
   - at least once per month, restore to a test environment.

---

## 4) Backup automation (scripts/tools)

A simple production approach: `bash` + `cron` + `restic`.

### 4.1 Example Linux script

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

tar -czf "$ARCHIVE" \
  -C "$SRC_APP" Data \
  "$SRC_CFG1" \
  "$SRC_CFG2"

sha256sum "$ARCHIVE" > "$MANIFEST"

# Optional: upload to S3/Blob/restic repository
# restic -r s3:s3.amazonaws.com/my-backups backup "$ARCHIVE" "$MANIFEST"

echo "Backup created: $ARCHIVE"
```

### 4.2 Scheduling with cron

```cron
0 */4 * * * /usr/local/bin/carcomparison-backup.sh >> /var/log/carcomparison-backup.log 2>&1
```

### 4.3 Recommended tools

- `restic` — encryption, deduplication, retention
- `borg` — efficient snapshot backups
- `rclone` — cloud copy/sync
- Cloud-native options: AWS Backup / Azure Backup / GCP Backup

---

## 5) Restore procedure

## 5.1 Full system restore

Scenario: complete server loss or major corruption.

Steps:
1. Prepare new server (OS, .NET runtime, Nginx, systemd).
2. Stop API service:
   ```bash
   sudo systemctl stop carcomparison-api
   ```
3. Restore application artifacts to `/opt/carcomparison/api`.
4. Restore `Data/` from backup archive.
5. Restore `systemd` unit and Nginx config.
6. Run `systemctl daemon-reload`, start services.
7. Verify endpoints (`/api/test`, `/swagger`).

## 5.2 Selective data restore

Scenario: only specific files are damaged (for example `users.json`).

Steps:
1. Stop service (or switch to read-only mode).
2. Extract required backup to temporary directory.
3. Replace only required files (`users.json`, etc.).
4. Validate JSON.
5. Start service and verify behavior.

## 5.3 Restore testing

Mandatory practice:
- Run monthly test restore in isolated environment.
- Track RTO/RPO:
  - **RTO** — time to restore service;
  - **RPO** — acceptable data loss window.
- Publish a short report: date, backup version, result, issues.

---

## Release Engineer / DevOps checklist

- [ ] 3-2-1 strategy implemented
- [ ] Full/incremental/differential schedule configured
- [ ] `Data/`, configs, and logs are included in backups
- [ ] Retention and encryption configured
- [ ] Checksums and archive validation implemented
- [ ] Regular test restores executed
- [ ] RTO/RPO and ownership documented
