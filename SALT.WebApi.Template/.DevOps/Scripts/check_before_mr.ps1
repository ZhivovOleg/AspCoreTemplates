param(
    [int]$CoverageLineThreshold = 1
)

$ErrorActionPreference = "Stop";

function Assert-CommandExists {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Command
    )

    if ($null -eq (Get-Command $Command -ErrorAction SilentlyContinue)) {
        throw "Required command was not found: $Command";
    }
}

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Command,

        [Parameter(ValueFromRemainingArguments = $true)]
        [string[]]$Arguments
    )

    & $Command @Arguments;
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code $($LASTEXITCODE): $Command $($Arguments -join ' ')";
    }
}

Assert-CommandExists dotnet;
Assert-CommandExists git;
Assert-CommandExists gitleaks;

[string]$ProjectDirectory = Resolve-Path (Join-Path $PSScriptRoot "../..");
[string]$SolutionFile = Join-Path $ProjectDirectory "SALT.WebApi.Template.sln";
[string]$ProjectFile = Join-Path $ProjectDirectory "src/SALT.WebApi.Template/SALT.WebApi.Template.csproj";
[string]$TestResultsDirectory = Join-Path $ProjectDirectory "TestResults";
[string]$GitleaksConfig = Join-Path $ProjectDirectory ".gitleaks.toml";

Push-Location $ProjectDirectory;

try {
    Remove-Item -Path $TestResultsDirectory -Recurse -Force -ErrorAction Ignore;

    Invoke-Checked dotnet restore $SolutionFile;
    Invoke-Checked dotnet build $SolutionFile --no-restore;

    Invoke-Checked dotnet list $ProjectFile package --vulnerable --include-transitive;

    Invoke-Checked dotnet test $SolutionFile --no-build --logger "trx" --collect:"XPlat Code Coverage" --results-directory $TestResultsDirectory -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover;

    & (Join-Path $PSScriptRoot "check_coverage.ps1") -ResultsDirectory $TestResultsDirectory -LineThreshold $CoverageLineThreshold;

    Invoke-Checked gitleaks protect --staged --redact --config $GitleaksConfig;
    Invoke-Checked gitleaks dir --redact --config $GitleaksConfig $ProjectDirectory;

    Write-Host "Pre-MR checks passed.";
}
finally {
    Pop-Location;
}
