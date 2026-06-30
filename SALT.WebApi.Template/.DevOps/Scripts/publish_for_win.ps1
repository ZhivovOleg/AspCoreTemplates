param(
    [string]$PublishDirectory = "c:/Release/",
    [int]$CoverageLineThreshold = 1
)

$ErrorActionPreference = "Stop";

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

[string]$ProjectDirectory = Resolve-Path (Join-Path $PSScriptRoot "../..");
[string]$ProjectFile = (Get-ChildItem -Path (Join-Path $ProjectDirectory "src") -Filter "*.csproj" -Recurse | Select-Object -First 1).FullName;
[string]$AppName = [System.IO.Path]::GetFileNameWithoutExtension($ProjectFile);
[string]$Runtime = "win-x64";
[string]$Version = Get-Date -Format "yyyy.MMdd.HHmm";
[string]$OutputDirectory = Join-Path $PublishDirectory $AppName;
[string]$ArchivePath = Join-Path $PublishDirectory "$($AppName).$($Runtime).$($Version).zip";
[string]$TestResultsDirectory = Join-Path $ProjectDirectory "TestResults";

Clear-Host;
Remove-Item -Path $OutputDirectory -Recurse -Force -ErrorAction Ignore;
Remove-Item -Path $TestResultsDirectory -Recurse -Force -ErrorAction Ignore;

Invoke-Checked dotnet clean $ProjectFile;
Invoke-Checked dotnet restore $ProjectFile;

Invoke-Checked dotnet list $ProjectFile package --outdated;
Invoke-Checked dotnet list $ProjectFile package --deprecated;
Invoke-Checked dotnet list $ProjectFile package --vulnerable --include-transitive;

Invoke-Checked dotnet build $ProjectFile -c Release --no-restore;

$TestProjects = Get-ChildItem -Path $ProjectDirectory -Recurse -Filter "*.csproj" |
    Where-Object {
        $_.FullName -notmatch "[\\/](bin|obj)[\\/]" -and
        $_.Name -match "(?i)(test|tests)\.csproj$"
    };

if ($TestProjects.Count -eq 0) {
    throw "No test projects found. Publishing without tests is not allowed.";
}
else {
    foreach ($TestProject in $TestProjects) {
        Invoke-Checked dotnet test $TestProject.FullName -c Release --logger "trx" --collect:"XPlat Code Coverage" --results-directory $TestResultsDirectory -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover;
    }

    & (Join-Path $PSScriptRoot "check_coverage.ps1") -ResultsDirectory $TestResultsDirectory -LineThreshold $CoverageLineThreshold;
}

Invoke-Checked dotnet publish $ProjectFile -c Release -o $OutputDirectory --runtime $Runtime --force -p:Version=$Version -p:OutputType=WinExe --no-self-contained --no-build;

Compress-Archive -Path "$OutputDirectory/*" -DestinationPath $ArchivePath -Force;
Remove-Item -Path $OutputDirectory -Recurse -Force -ErrorAction Ignore;

Write-Host "Artifact created: $ArchivePath";
