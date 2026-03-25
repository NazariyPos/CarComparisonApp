#!/usr/bin/env bash
set -euo pipefail

INCLUDE_TESTS="false"
if [[ "${1:-}" == "--include-tests" ]]; then
  INCLUDE_TESTS="true"
fi

echo "[verify] Restoring tools"
dotnet tool restore

echo "[verify] Restoring dependencies"
dotnet restore CarComparisonApi/CarComparisonApi.csproj
dotnet restore CarComparisonApp.Tests/CarComparisonApp.Tests.csproj

echo "[verify] Running Roslynator"
dotnet roslynator analyze CarComparisonApi/CarComparisonApi.csproj CarComparisonApp.Tests/CarComparisonApp.Tests.csproj

echo "[verify] Running documentation quality checks"
./scripts/verify-docs.sh

echo "[verify] Building projects"
dotnet build CarComparisonApi/CarComparisonApi.csproj --no-restore
dotnet build CarComparisonApp.Tests/CarComparisonApp.Tests.csproj --no-restore

if [[ "$INCLUDE_TESTS" == "true" ]]; then
  echo "[verify] Running tests"
  dotnet test CarComparisonApp.Tests/CarComparisonApp.Tests.csproj --no-build --no-restore
fi

echo "[verify] All checks passed"
