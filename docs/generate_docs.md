# Documentation Generation

This instruction describes how to generate documentation in the project using `Swashbuckle (Swagger)` and `DocFX`.

## Prerequisites

- Installed `.NET SDK 8+`
- Navigate to the repository root:

```bash
cd CarComparisonApp
```

## 1) Generate OpenAPI (Swagger) JSON

Swagger JSON is generated from the running API.

### Option A: Manually

1. Start the API:

```bash
dotnet run --project CarComparisonApi/CarComparisonApi.csproj --urls http://localhost:5056
```

2. In another terminal, save the Swagger JSON:

```bash
curl http://localhost:5056/swagger/v1/swagger.json -o docs/swagger.json
```

### Option B: PowerShell (automatically start/stop)

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

## 2) Generate the DocFX Documentation Site

1. Install DocFX locally into the .tools folder (one‑time):

```bash
dotnet tool install docfx --tool-path ./.tools
```

2. Run the generation using the docfx.json configuration:

```bash
./.tools/docfx docfx.json
```

After successful execution:
- API metadata will be in api/
- the static documentation site will be in _site/

## 3) Documentation Quality Check

Run DocFX validation with warnings treated as errors:

```bash
./.tools/docfx build docfx.json --warningsAsErrors
```

Convenience scripts:

- PowerShell:
```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify-docs.ps1
```
- Bash:
```bash
./scripts/verify-docs.sh
```

## 4) View the Generated Documentation

Open in a browser:

- `_site/index.html`

## 5) Documentation Archive (optional)

To create an archive for distribution / upload:

```powershell
if (Test-Path documentation-archive.zip) { Remove-Item documentation-archive.zip -Force }
Compress-Archive -Path _site,docs/swagger.json,docfx.json,docs/index.md,docs/toc.yml -DestinationPath documentation-archive.zip
```

Result: documentation-archive.zip in the repository root.
