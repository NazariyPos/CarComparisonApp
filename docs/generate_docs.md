# Documentation Generation

This guide describes how to generate project documentation using `Swashbuckle (Swagger)` and `DocFX`, and how it is published via GitHub Pages.

## Prerequisites

- Installed `.NET SDK 8+`
- Navigate to the repository root:

```bash
cd CarComparisonApp
```

## 1) Generate OpenAPI (Swagger) JSON

Swagger JSON is generated from a running API instance.

### Option A: Manual

1. Start the API:

```bash
dotnet run --project CarComparisonApi/CarComparisonApi.csproj --urls http://localhost:5056
```

2. In another terminal, save the Swagger JSON:

```bash
curl http://localhost:5056/swagger/v1/swagger.json -o docs/swagger.json
```

### Option B: PowerShell (automatic start/stop)

```powershell
$p = Start-Process dotnet -ArgumentList 'run --project CarComparisonApi/CarComparisonApi.csproj --urls http://localhost:5056' -PassThru
Start-Sleep -Seconds 8
try {
    Invoke-WebRequest -UseBasicParsing http://localhost:5056/swagger/v1/swagger.json -OutFile docs/swagger.json
}
finally {
    Stop-Process -Id $p.Id -Force
}
```

## 2) Generate the DocFX documentation site

1. Install `DocFX` locally into the `.tools` folder (one-time):

```bash
dotnet tool install docfx --tool-path ./.tools
```

2. Run generation using `docfx.json`:

```bash
./.tools/docfx docfx.json
```

After successful execution:
- API metadata is generated in `api/`
- the static documentation site is generated in `_site/`

## 3) Documentation quality check

Run DocFX validation with warnings treated as errors:

```bash
./.tools/docfx build docfx.json --warningsAsErrors
```

Quick helper scripts:

PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify-docs.ps1
```

Bash:
```bash
./scripts/verify-docs.sh
```

## 4) Publish via GitHub Pages (CI/CD)

CI/CD workflow file:
- `.github/workflows/docs-pages.yml`

What the workflow does:
1. runs on `push` to `main` (or manually);
2. builds the DocFX site;
3. publishes `_site` to GitHub Pages via GitHub Actions.

One-time repository setup:
1. Open `Settings` -> `Pages`.
2. Set `Source` to `GitHub Actions`.

After the first successful deployment, documentation will be available at:
- `https://<github-username>.github.io/CarComparisonApp/`

## 5) View generated documentation locally

Open in a browser:

- `_site/index.html`

## 6) Documentation archive (optional)

To create an archive for sharing/submission:

```powershell
if (Test-Path documentation-archive.zip) { Remove-Item documentation-archive.zip -Force }
Compress-Archive -Path _site,docs/swagger.json,docfx.json,docs/index.md,docs/toc.yml -DestinationPath documentation-archive.zip
```

Result: `documentation-archive.zip` in the repository root.

## Git note

The `api/` and `_site/` folders are generated artifacts and usually should not be committed to the repository.
