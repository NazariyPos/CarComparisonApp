#!/usr/bin/env bash
set -euo pipefail

DOCFX="./.tools/docfx"
if [[ ! -f "$DOCFX" ]]; then
  echo "[docs] Installing DocFX locally"
  dotnet tool install docfx --tool-path ./.tools >/dev/null
fi

echo "[docs] Building documentation with warnings as errors"
"$DOCFX" build docfx.json --warningsAsErrors

echo "[docs] Documentation quality checks passed"
