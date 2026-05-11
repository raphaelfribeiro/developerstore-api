# coverage-report.ps1 — Generate HTML coverage report on Windows
# Usage: .\coverage-report.ps1
# Requirements: dotnet-reportgenerator-globaltool
#   Install: dotnet tool install -g dotnet-reportgenerator-globaltool

$BackendDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$CoverageDir = Join-Path $BackendDir "coverage-results"
$ReportDir = Join-Path $BackendDir "coverage-report"

Write-Host "Cleaning previous results..." -ForegroundColor Yellow
if (Test-Path $CoverageDir) { Remove-Item -Recurse -Force $CoverageDir }
if (Test-Path $ReportDir) { Remove-Item -Recurse -Force $ReportDir }
New-Item -ItemType Directory -Force -Path $CoverageDir | Out-Null

Write-Host "Running tests with coverage..." -ForegroundColor Yellow
dotnet test "$BackendDir\Ambev.DeveloperEvaluation.sln" `
  --configuration Release `
  --collect:"XPlat Code Coverage" `
  --results-directory $CoverageDir `
  --filter "FullyQualifiedName~Unit" `
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura `
     DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include="[Ambev.DeveloperEvaluation.Domain]*,[Ambev.DeveloperEvaluation.Application]*"

Write-Host "Generating HTML report..." -ForegroundColor Yellow
reportgenerator `
  "-reports:$CoverageDir\**\coverage.cobertura.xml" `
  "-targetdir:$ReportDir" `
  "-reporttypes:Html;TextSummary" `
  "-assemblyfilters:+Ambev.DeveloperEvaluation.Domain;+Ambev.DeveloperEvaluation.Application"

Write-Host ""
Write-Host "Coverage report generated at: $ReportDir\index.html" -ForegroundColor Green
Write-Host ""

$summaryPath = Join-Path $ReportDir "Summary.txt"
if (Test-Path $summaryPath) { Get-Content $summaryPath }
