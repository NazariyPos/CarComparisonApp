param(
    [switch]$IncludeTests
)

$ErrorActionPreference = 'Stop'

function Invoke-Dotnet {
    param([string[]]$Arguments)

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

Write-Host "[verify] Restoring tools"
Invoke-Dotnet @("tool", "restore")

Write-Host "[verify] Restoring dependencies"
Invoke-Dotnet @("restore", "CarComparisonApi/CarComparisonApi.csproj")
Invoke-Dotnet @("restore", "CarComparisonApp.Tests/CarComparisonApp.Tests.csproj")

Write-Host "[verify] Running Roslynator"
Invoke-Dotnet @("roslynator", "analyze", "CarComparisonApi/CarComparisonApi.csproj", "CarComparisonApp.Tests/CarComparisonApp.Tests.csproj")

Write-Host "[verify] Building projects"
Invoke-Dotnet @("build", "CarComparisonApi/CarComparisonApi.csproj", "--no-restore")
Invoke-Dotnet @("build", "CarComparisonApp.Tests/CarComparisonApp.Tests.csproj", "--no-restore")

if ($IncludeTests) {
    Write-Host "[verify] Running tests"
    Invoke-Dotnet @("test", "CarComparisonApp.Tests/CarComparisonApp.Tests.csproj", "--no-build", "--no-restore")
}

Write-Host "[verify] All checks passed"
