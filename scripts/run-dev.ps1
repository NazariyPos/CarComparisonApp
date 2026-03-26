param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$DotnetArgs
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptDir "..")
$projectPath = Join-Path $repoRoot "CarComparisonApi/CarComparisonApi.csproj"

$env:ASPNETCORE_ENVIRONMENT = "Development"

Write-Host "[run-dev] Starting API in Development mode"
Write-Host "[run-dev] Project: $projectPath"

& dotnet watch --project $projectPath run @DotnetArgs
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
