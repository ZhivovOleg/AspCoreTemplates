#!/usr/bin/env bash
set -euo pipefail

results_directory="${1:-TestResults}"
line_threshold="${2:-${COVERAGE_LINE_THRESHOLD:-1}}"

if [[ ! -d "$results_directory" ]]; then
  echo "Coverage results directory was not found: $results_directory" >&2
  exit 1
fi

mapfile -t coverage_reports < <(
  find "$results_directory" \
    -type f \
    -name "coverage.opencover.xml" \
    -print
)

if [[ "${#coverage_reports[@]}" -eq 0 ]]; then
  echo "No OpenCover coverage reports found in $results_directory" >&2
  exit 1
fi

visited_sequence_points=0
total_sequence_points=0

for report in "${coverage_reports[@]}"; do
  summary="$(grep -m 1 "<Summary " "$report" || true)"
  visited="$(printf '%s\n' "$summary" | sed -n 's/.*visitedSequencePoints="\([0-9][0-9]*\)".*/\1/p')"
  total="$(printf '%s\n' "$summary" | sed -n 's/.*numSequencePoints="\([0-9][0-9]*\)".*/\1/p')"

  if [[ -z "$visited" || -z "$total" ]]; then
    echo "Cannot read OpenCover summary from $report" >&2
    exit 1
  fi

  visited_sequence_points=$((visited_sequence_points + visited))
  total_sequence_points=$((total_sequence_points + total))
done

if [[ "$total_sequence_points" -eq 0 ]]; then
  echo "Coverage reports do not contain sequence points" >&2
  exit 1
fi

line_coverage="$(awk -v visited="$visited_sequence_points" -v total="$total_sequence_points" 'BEGIN { printf "%.2f", (visited / total) * 100 }')"

echo "Line coverage: $line_coverage% ($visited_sequence_points/$total_sequence_points sequence points). Threshold: $line_threshold%."

awk -v coverage="$line_coverage" -v threshold="$line_threshold" 'BEGIN { exit (coverage >= threshold) ? 0 : 1 }' || {
  echo "Coverage is below threshold" >&2
  exit 1
}
