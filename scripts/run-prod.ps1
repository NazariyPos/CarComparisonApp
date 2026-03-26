param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$DotnetArgs
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptDir "..")
$projectPath = Join-Path $repoRoot "CarComparisonApi/CarComparisonApi.csproj"
$publishDir = if ($env:PUBLISH_DIR) { $env:PUBLISH_DIR } else { Join-Path $repoRoot "publish/api" }

if ([string]::IsNullOrWhiteSpace($env:Jwt__Key)) {
    Write-Host "[run-prod] Jwt__Key is required for production run"
    exit 1
}

$env:ASPNETCORE_ENVIRONMENT = "Production"
if ([string]::IsNullOrWhiteSpace($env:ASPNETCORE_URLS)) {
    $env:ASPNETCORE_URLS = "http://127.0.0.1:5060"
}

Write-Host "[run-prod] Publishing API to $publishDir"
& dotnet publish $projectPath -c Release -o $publishDir
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$dllPath = Join-Path $publishDir "CarComparisonApi.dll"

Write-Host "[run-prod] Starting API in Production mode"
& dotnet $dllPath @DotnetArgs
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
