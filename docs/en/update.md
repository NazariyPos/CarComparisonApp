# System Update Guide (Release Engineer / DevOps)

This document describes a clear and safe process for updating `CarComparisonApi` in production.

> Current project state: data is stored in JSON files (`CarComparisonApi/Data/*.json`), and no separate DB engine is used.

---

## 1) Preparation for update

### 1.1 Create backups

Before any update, create backups of:

1. **Current application release** (app folder):
```bash
sudo tar -czf /opt/backups/carcomparison/app_$(date +%F_%H%M).tar.gz /opt/carcomparison/api
```

2. **JSON data** (critical):
```bash
sudo tar -czf /opt/backups/carcomparison/data_$(date +%F_%H%M).tar.gz /opt/carcomparison/api/Data
```

3. **Configuration** (`systemd`, `nginx`):
```bash
sudo tar -czf /opt/backups/carcomparison/config_$(date +%F_%H%M).tar.gz \
  /etc/systemd/system/carcomparison-api.service \
  /etc/nginx/sites-available/carcomparison
```

4. Verify backup files exist:
```bash
ls -lah /opt/backups/carcomparison
```

### 1.2 Compatibility checks

Before deployment, verify:

- `.NET Runtime` version on server:
```bash
dotnet --info
```
- new build targets `net8.0`;
- JSON format compatibility (no breaking field removals without migration);
- environment variable compatibility (`Jwt__*` and any new settings).

A staging smoke test before production is recommended.

### 1.3 Downtime planning

Recommendations:
- schedule a maintenance window (e.g., 5–15 min);
- notify team/users;
- prepare rollback commands in advance.

---

## 2) Update process

### 2.1 Stop required services

For single-instance deployment:
```bash
sudo systemctl stop carcomparison-api
sudo systemctl status carcomparison-api
```

`nginx` can remain running (it will return `502` while backend is unavailable), or you can enable a maintenance page.

### 2.2 Deploy new code

Recommended approach: **release folders + symlink**.

1. Copy new build:
```bash
sudo mkdir -p /opt/carcomparison/releases/2026-01-15_1200
# copy artifacts into this folder
```

2. Switch `current` symlink:
```bash
sudo ln -sfn /opt/carcomparison/releases/2026-01-15_1200 /opt/carcomparison/current
```

3. Ensure `systemd` uses `current` for `WorkingDirectory/ExecStart`.

> If symlinks are not used, update `/opt/carcomparison/api` atomically (temp folder + move).

### 2.3 Update configuration

Verify:
- `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`;
- `ASPNETCORE_ENVIRONMENT=Production`;
- `ASPNETCORE_URLS=http://127.0.0.1:5060`.

If `systemd` unit or `nginx` config changed:
```bash
sudo systemctl daemon-reload
sudo nginx -t
sudo systemctl reload nginx
```

### 2.4 Start after update

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

## 3) Detailed rollback procedure (failed update)

Rollback should be triggered if:
- service does not start;
- repeated 5xx after release;
- critical features fail;
- data format issues appear.

### 3.1 Rollback trigger criteria

Treat release as failed if within 5–10 minutes after deployment:
- `systemctl status` shows `failed/restarting`;
- `/api/test` is not `200`;
- `journalctl` shows recurring critical exceptions;
- critical business smoke tests fail.

### 3.2 Fast code rollback (symlink strategy)

1. Stop service:
```bash
sudo systemctl stop carcomparison-api
```

2. Switch `current` back to previous release:
```bash
sudo ln -sfn /opt/carcomparison/releases/<previous_release> /opt/carcomparison/current
```

3. Start service:
```bash
sudo systemctl start carcomparison-api
sudo systemctl status carcomparison-api
```

4. Verify health:
```bash
curl -i http://127.0.0.1:5060/api/test
```

### 3.3 Rollback without symlinks (from backup archives)

1. Stop service:
```bash
sudo systemctl stop carcomparison-api
```

2. Restore code:
```bash
sudo rm -rf /opt/carcomparison/api
sudo mkdir -p /opt/carcomparison/api
sudo tar -xzf /opt/backups/carcomparison/app_<timestamp>.tar.gz -C /
```

3. Restore data if needed:
```bash
sudo tar -xzf /opt/backups/carcomparison/data_<timestamp>.tar.gz -C /
```

4. Start service and re-check.

### 3.4 Configuration rollback

If issue is configuration-related:

```bash
sudo tar -xzf /opt/backups/carcomparison/config_<timestamp>.tar.gz -C /
sudo systemctl daemon-reload
sudo nginx -t
sudo systemctl reload nginx
sudo systemctl restart carcomparison-api
```

### 3.5 Mandatory post-rollback actions

1. Record incident details (time, release version, symptoms).
2. Export logs:
```bash
sudo journalctl -u carcomparison-api -n 500 --no-pager > /tmp/carcomparison_rollback.log
```
3. Mark release as failed in changelog/tracker.
4. Start RCA and prepare corrected re-release plan.
