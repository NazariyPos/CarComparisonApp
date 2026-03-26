#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT_PATH="$REPO_ROOT/CarComparisonApi/CarComparisonApi.csproj"
PUBLISH_DIR="${PUBLISH_DIR:-$REPO_ROOT/publish/api}"

if [[ -z "${Jwt__Key:-}" ]]; then
  echo "[run-prod] Jwt__Key is required for production run"
  exit 1
fi

export ASPNETCORE_ENVIRONMENT="Production"
export ASPNETCORE_URLS="${ASPNETCORE_URLS:-http://127.0.0.1:5060}"

echo "[run-prod] Publishing API to $PUBLISH_DIR"
dotnet publish "$PROJECT_PATH" -c Release -o "$PUBLISH_DIR"

echo "[run-prod] Starting API in Production mode"
dotnet "$PUBLISH_DIR/CarComparisonApi.dll" "$@"
