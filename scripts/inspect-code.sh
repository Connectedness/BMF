#!/usr/bin/env bash

set -euo pipefail

script_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd -- "$script_directory/.." && pwd)"
severity="${1:-WARNING}"
target="${2:-$repository_root/BrilliantMessaging.slnx}"
report_directory="$repository_root/artifacts/inspect-code"
report_file="$report_directory/inspection-report.xml"

# InspectCode reuses its analysis caches across runs and can report stale results
# after the sources changed. A throwaway caches home makes every run reproducible.
caches_home="$(mktemp -d)"
trap 'rm -rf -- "$caches_home"' EXIT

mkdir -p -- "$report_directory"

# Solution-wide analysis (--swea) is required for the inspections that can only be
# decided across projects, e.g. unused non-private members. Release is inspected
# because that is the configuration whose warnings CI treats as errors.
dotnet tool restore
dotnet jb inspectcode "$target" \
    --output="$report_file" \
    --format=Xml \
    --severity="$severity" \
    --swea \
    --properties:Configuration=Release \
    --no-updates \
    --caches-home="$caches_home"

echo "Inspection report was written to $report_file."
