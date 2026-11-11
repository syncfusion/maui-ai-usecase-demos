#!/usr/bin/env bash
# Verifies the MAUI migration: unit tests, designs loaded into the real SurfaceHost, and an Android (net10.0) build.
# Usage: harness/verify.sh [--no-android]
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

echo "== SDK =="; dotnet --version

echo "== Unit tests (SkyBook.Core) =="
dotnet test tests/SkyBook.Core.Tests --nologo

echo "== MAUI designs load into Syncfusion.Maui.A2UI SurfaceHost =="
dotnet test tests/SkyBook.Designs.Tests --nologo

echo "== Agent designs are valid JSON =="
for f in ../../agent/flight_booking_agent/designs_maui/*.json; do python3 -m json.tool "$f" >/dev/null && echo "ok $(basename "$f")"; done

if [[ "${1:-}" != "--no-android" ]]; then
  echo "== Build SkyBook.Maui (net10.0-android, warnings as errors) =="
  dotnet build src/SkyBook.Maui -f net10.0-android --nologo
fi
echo "ALL CHECKS PASSED"
