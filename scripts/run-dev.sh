#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT_PATH="$REPO_ROOT/CarComparisonApi/CarComparisonApi.csproj"

export ASPNETCORE_ENVIRONMENT="Development"

echo "[run-dev] Starting API in Development mode"
echo "[run-dev] Project: $PROJECT_PATH"

dotnet watch --project "$PROJECT_PATH" run "$@"
