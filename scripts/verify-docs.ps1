$ErrorActionPreference = 'Stop'

$docfxPath = Join-Path (Get-Location) ".tools/docfx.exe"

if (-not (Test-Path $docfxPath)) {
    Write-Host "[docs] Installing DocFX locally"
    dotnet tool install docfx --tool-path ./.tools | Out-Null
}

Write-Host "[docs] Building documentation with warnings as errors"
& $docfxPath build docfx.json --warningsAsErrors
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$swaggerPath = "docs/swagger.json"
if (Test-Path $swaggerPath) {
    Write-Host "[docs] Validating docs/swagger.json"
    $null = Get-Content $swaggerPath -Raw | ConvertFrom-Json
}

Write-Host "[docs] Documentation quality checks passed"
