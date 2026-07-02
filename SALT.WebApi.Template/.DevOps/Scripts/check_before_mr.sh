#!/usr/bin/env bash
set -euo pipefail

script_directory="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
project_directory="$(cd "$script_directory/../.." && pwd)"
solution_file="$project_directory/SALT.WebApi.Template.sln"
project_file="$project_directory/src/SALT.WebApi.Template/SALT.WebApi.Template.csproj"
test_results_directory="$project_directory/TestResults"
coverage_threshold="${COVERAGE_LINE_THRESHOLD:-1}"
gitleaks_config="$project_directory/.gitleaks.toml"

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Required command was not found: $1" >&2
    exit 1
  fi
}

require_command dotnet
require_command git
require_command gitleaks

cd "$project_directory"

rm -rf "$test_results_directory"

dotnet restore "$solution_file"
dotnet build "$solution_file" --no-restore

dotnet list "$project_file" package --vulnerable --include-transitive

dotnet test "$solution_file" \
  --no-build \
  --logger "trx" \
  --collect:"XPlat Code Coverage" \
  --results-directory "$test_results_directory" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

"$script_directory/check_coverage.sh" "$test_results_directory" "$coverage_threshold"

# Scan staged changes before commit. The command is hidden in newer Gitleaks,
# but it is still supported and convenient for pre-MR checks.
gitleaks protect --staged --redact --config "$gitleaks_config"

# Scan the current working tree too, so already committed local changes are checked before MR.
gitleaks dir --redact --config "$gitleaks_config" "$project_directory"

echo "Pre-MR checks passed."
