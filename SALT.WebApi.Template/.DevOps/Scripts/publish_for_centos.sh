#!/usr/bin/env bash
set -euo pipefail

publish_directory="${1:-/tmp/release}"
script_directory="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
project_directory="$(cd "$script_directory/../.." && pwd)"
project_file="$(find "$project_directory/src" -mindepth 2 -maxdepth 2 -name "*.csproj" | head -n 1)"

if [[ -z "$project_file" ]]; then
  echo "Project file was not found in $project_directory" >&2
  exit 1
fi

app_name="$(basename "$project_file" .csproj)"
runtime="linux-x64"
version="$(date '+%Y.%m%d.%H%M')"
output_directory="$publish_directory/$app_name"
archive_path="$publish_directory/$app_name.$runtime.$version.zip"
test_results_directory="$project_directory/TestResults"
coverage_threshold="${COVERAGE_LINE_THRESHOLD:-1}"

rm -rf "$output_directory" "$test_results_directory"
mkdir -p "$publish_directory"

dotnet clean "$project_file"
dotnet restore "$project_file"

dotnet list "$project_file" package --outdated
dotnet list "$project_file" package --deprecated
dotnet list "$project_file" package --vulnerable --include-transitive

dotnet build "$project_file" -c Release --no-restore

mapfile -t test_projects < <(
  find "$project_directory" \
    -path "*/bin" -prune -o \
    -path "*/obj" -prune -o \
    -iname "*test*.csproj" -print
)

if [[ "${#test_projects[@]}" -eq 0 ]]; then
  echo "No test projects found. Publishing without tests is not allowed." >&2
  exit 1
else
  for test_project in "${test_projects[@]}"; do
    dotnet test "$test_project" \
      -c Release \
      --logger "trx" \
      --collect:"XPlat Code Coverage" \
      --results-directory "$test_results_directory" \
      -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
  done

  "$script_directory/check_coverage.sh" "$test_results_directory" "$coverage_threshold"
fi

dotnet publish "$project_file" \
  -c Release \
  -o "$output_directory" \
  --runtime "$runtime" \
  --force \
  -p:Version="$version" \
  --no-self-contained \
  --no-build

(cd "$output_directory" && zip -qr "$archive_path" .)
rm -rf "$output_directory"

echo "Artifact created: $archive_path"
