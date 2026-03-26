# Production Deployment Guide (Release Engineer / DevOps)

This guide describes how to deploy `CarComparisonApi` to a production environment.

## 1) Hardware requirements

### Supported architecture
- `x64` Linux server (recommended).

### Minimum requirements (small environment)
- CPU: `2 vCPU`
- RAM: `4 GB`
- Disk: `20 GB SSD`

### Recommended requirements (stable production)
- CPU: `4 vCPU`
- RAM: `8 GB`
- Disk: `50+ GB SSD`

## 2) Required software

Install on target server:
- `.NET 8 Runtime` (or SDK if building on server)
- `Nginx` (reverse proxy + TLS termination)
- `systemd` (service management, included in Ubuntu)

## 3) Network setup

Open inbound ports:
- `80/tcp` (HTTP for redirect/challenges)
- `443/tcp` (HTTPS)
- `22/tcp` (SSH, restricted)

Recommended:
- expose only `Nginx` publicly;
- run API on localhost only (e.g. `127.0.0.1:5060`).

## 4) Server configuration

### 4.1 Create deployment directories

```bash
sudo mkdir -p /opt/carcomparison/api
sudo mkdir -p /var/log/carcomparison
sudo chown -R $USER:$USER /opt/carcomparison /var/log/carcomparison
```

### 4.2 Publish application

From CI or local build machine:

```bash
dotnet publish CarComparisonApi/CarComparisonApi.csproj -c Release -o ./publish/api
```

Copy published artifacts to server:
- `/opt/carcomparison/api`

### 4.3 Configure environment variables

Set production secrets via environment (recommended), for example in systemd unit:
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://127.0.0.1:5060`

### 4.4 Create systemd service

Example: `/etc/systemd/system/carcomparison-api.service`

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

Apply service:

```bash
sudo systemctl daemon-reload
sudo systemctl enable carcomparison-api
sudo systemctl start carcomparison-api
sudo systemctl status carcomparison-api
```

### 4.5 Configure Nginx reverse proxy

Example: `/etc/nginx/sites-available/carcomparison`

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

Enable config:

```bash
sudo ln -s /etc/nginx/sites-available/carcomparison /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

Then configure TLS (e.g. Let's Encrypt) and force HTTPS.

## 5) Database setup

Current status:
- No relational/NoSQL DB is used by design.
- App data is stored in JSON files under `CarComparisonApi/Data/`.

Production recommendations for current storage approach:
- keep JSON files on persistent disk;
- set correct file permissions for service user;
- include these files in backup policy;
- consider migration to a DB if multi-instance scaling is required.

## 6) Code deployment process

Recommended rollout sequence:
1. Build and test in CI.
2. Publish release artifacts (`dotnet publish -c Release`).
3. Upload artifacts to server (`/opt/carcomparison/api`).
4. Restart service:
   ```bash
   sudo systemctl restart carcomparison-api
   ```
5. Verify health checks and logs.

Rollback strategy:
- keep previous release directory;
- switch symlink/folder back;
- restart systemd service.

## 7) Post-deployment verification

### Functional checks

- API health endpoint (if exposed):
  - `GET /api/test` should return `200 OK`.
- Swagger/OpenAPI:
  - `GET /swagger/v1/swagger.json` should return `200 OK`.
  - `/swagger` should load.

### Service checks

```bash
sudo systemctl status carcomparison-api
sudo journalctl -u carcomparison-api -n 200 --no-pager
```

### Nginx checks

```bash
sudo nginx -t
sudo systemctl status nginx
```

### Success criteria

Deployment is considered successful when:
- `carcomparison-api` service is active and stable;
- API endpoints respond with expected status codes;
- Swagger UI and OpenAPI JSON are reachable;
- no critical errors appear in API/Nginx logs.
