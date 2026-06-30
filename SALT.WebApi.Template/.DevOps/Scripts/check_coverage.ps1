param(
    [Parameter(Mandatory = $true)]
    [string]$ResultsDirectory,

    [int]$LineThreshold = 1
)

$ErrorActionPreference = "Stop";

if (-not (Test-Path $ResultsDirectory)) {
    throw "Coverage results directory was not found: $ResultsDirectory";
}

$CoverageReports = Get-ChildItem -Path $ResultsDirectory -Recurse -Filter "coverage.opencover.xml";

if ($CoverageReports.Count -eq 0) {
    throw "No OpenCover coverage reports found in $ResultsDirectory";
}

[int]$VisitedSequencePoints = 0;
[int]$TotalSequencePoints = 0;

foreach ($CoverageReport in $CoverageReports) {
    [xml]$CoverageXml = Get-Content -Path $CoverageReport.FullName;
    $Summary = $CoverageXml.CoverageSession.Summary;

    if ($null -eq $Summary) {
        throw "Cannot read OpenCover summary from $($CoverageReport.FullName)";
    }

    $VisitedSequencePoints += [int]$Summary.visitedSequencePoints;
    $TotalSequencePoints += [int]$Summary.numSequencePoints;
}

if ($TotalSequencePoints -eq 0) {
    throw "Coverage reports do not contain sequence points";
}

[double]$LineCoverage = [Math]::Round(($VisitedSequencePoints / $TotalSequencePoints) * 100, 2);

Write-Host "Line coverage: $LineCoverage% ($VisitedSequencePoints/$TotalSequencePoints sequence points). Threshold: $LineThreshold%.";

if ($LineCoverage -lt $LineThreshold) {
    throw "Coverage is below threshold";
}
