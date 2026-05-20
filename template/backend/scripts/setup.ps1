<#
.SYNOPSIS
    Full environment setup, build, test and teardown for Ambev Developer Evaluation.

.DESCRIPTION
    Bootstraps the development environment from zero: checks prerequisites,
    restores and builds the .NET solution, runs all test suites, generates
    coverage reports, builds and starts Docker services, executes end-to-end
    API tests, and optionally tears down containers.

    Every step produces a dedicated report file under ./setup-reports/<run-id>/.
    At the end a rich summary table is printed and the window waits for ENTER.

.PARAMETER Step
    Step to execute. Accepts the step NAME ("UnitTests") or step NUMBER (4).
    Omit (or pass "All") to run everything.

    Step list:
       1  Prerequisites    Verify dotnet, docker, python, reportgenerator
       2  Restore          dotnet restore
       3  Build            dotnet build --configuration Release
       4  UnitTests        dotnet test (unit suite only)
       5  Coverage         Unit tests + HTML coverage via reportgenerator
       6  IntegrationTests dotnet test (integration suite, Testcontainers)
       7  FunctionalTests  dotnet test (functional suite)
       8  DockerBuild      docker-compose build
       9  DockerUp         docker-compose up -d + wait for API
      10  ApiTest          python scripts/test_api.py
      11  DockerDown       docker-compose down -v

.PARAMETER From
    Switch. When combined with -Step <name|number>, runs from that step to the
    end (instead of running only that single step).

    Examples:
        .\setup.ps1 -Step 8 -From        # DockerBuild through DockerDown
        .\setup.ps1 -Step DockerBuild -From

.PARAMETER KeepContainers
    Switch. Skips the DockerDown step at the end of a full run or -From run.

.PARAMETER RunId
    Optional label for this execution (report sub-folder). Defaults to timestamp.

.EXAMPLE
    .\setup.ps1

.EXAMPLE
    .\setup.ps1 -KeepContainers

.EXAMPLE
    .\setup.ps1 -Step 4

.EXAMPLE
    .\setup.ps1 -Step Coverage

.EXAMPLE
    .\setup.ps1 -Step 8 -From

.EXAMPLE
    .\setup.ps1 -Step DockerBuild -From -KeepContainers

.NOTES
    Requirements:
      .NET SDK 8.0+        https://dot.net
      Docker Desktop       https://www.docker.com/products/docker-desktop
      Python 3.x           https://www.python.org  (ApiTest step only)
      reportgenerator      auto-installed by Prerequisites step

    Exit codes:  0 = all steps passed   1 = one or more steps failed
#>

[CmdletBinding()]
param(
    [string]$Step = "All",   # step name ("UnitTests") or number (4) or "All"
    [switch]$From,           # run from -Step to the end
    [switch]$KeepContainers,
    [string]$RunId = (Get-Date -Format "yyyyMMdd-HHmmss")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# STEP REGISTRY
# Single source of truth for step names and their execution order.
# ---------------------------------------------------------------------------

$script:StepNames = @(
    "Prerequisites", "Restore", "Build", "UnitTests", "Coverage",
    "IntegrationTests", "FunctionalTests", "DockerBuild", "DockerUp",
    "ApiTest", "DockerDown"
)

# Number -> Name map (1-based)
$script:StepByNumber = @{}
for ($i = 0; $i -lt $script:StepNames.Count; $i++) {
    $script:StepByNumber[$i + 1] = $script:StepNames[$i]
}

# Resolve -Step: accept number (1-11) or name or "All"
$parsedNum = 0
if ($Step -ne "All" -and [int]::TryParse($Step, [ref]$parsedNum)) {
    if (-not $script:StepByNumber.ContainsKey($parsedNum)) {
        Write-Error "Invalid step number '$Step'. Valid range: 1-$($script:StepNames.Count)."
        exit 1
    }
    $Step = $script:StepByNumber[$parsedNum]
}

$validSteps = @("All") + $script:StepNames
if ($Step -notin $validSteps) {
    Write-Error "Unknown step '$Step'. Valid steps: $($validSteps -join ', ')"
    exit 1
}

# -From only makes sense with a specific step, not with "All"
if ($From -and $Step -eq "All") {
    Write-Error "-From requires a specific -Step (name or number)."
    exit 1
}

# ---------------------------------------------------------------------------
# CONFIGURATION
# ---------------------------------------------------------------------------

$Root        = $PSScriptRoot
$Sln         = Join-Path $Root "..\Ambev.DeveloperEvaluation.sln"
$TestUnit    = Join-Path $Root "..\tests\Ambev.DeveloperEvaluation.Unit\Ambev.DeveloperEvaluation.Unit.csproj"
$TestInt     = Join-Path $Root "..\tests\Ambev.DeveloperEvaluation.Integration\Ambev.DeveloperEvaluation.Integration.csproj"
$TestFunc    = Join-Path $Root "..\tests\Ambev.DeveloperEvaluation.Functional\Ambev.DeveloperEvaluation.Functional.csproj"
$CoveragePs1 = Join-Path $Root "..\coverage-report.ps1"
$TestApiPy   = Join-Path $Root "test_api.py"
$ReportsBase = Join-Path $Root "..\setup-reports"
$ReportsDir  = Join-Path $ReportsBase $RunId

$ApiBaseUrl      = "http://localhost:8080"
$ApiReadyMaxSecs = 120
$ApiReadyPollMs  = 5000

# ---------------------------------------------------------------------------
# STATE
# ---------------------------------------------------------------------------

$script:StepResults   = [System.Collections.Generic.List[hashtable]]::new()
$script:StepCounter   = 0
$script:RunStartTime  = Get-Date
$script:LastMetrics   = ""

# ---------------------------------------------------------------------------
# DISPLAY HELPERS
# ---------------------------------------------------------------------------

function Write-Banner {
    param([string]$text, [string]$color = "Cyan")
    $bar = "=" * 72
    Write-Host ""
    Write-Host $bar -ForegroundColor $color
    Write-Host ("  " + $text) -ForegroundColor $color
    Write-Host $bar -ForegroundColor $color
}

function Write-Info {
    param([string]$msg, [string]$color = "White")
    $ts = (Get-Date).ToString("HH:mm:ss")
    Write-Host "  [$ts] $msg" -ForegroundColor $color
}

function Write-Ok {
    param([string]$msg)
    $ts = (Get-Date).ToString("HH:mm:ss")
    Write-Host "  [$ts] OK  $msg" -ForegroundColor Green
}

function Write-Warn {
    param([string]$msg)
    $ts = (Get-Date).ToString("HH:mm:ss")
    Write-Host "  [$ts] WARN  $msg" -ForegroundColor Yellow
}

function Write-Divider {
    Write-Host ("  " + ("-" * 68)) -ForegroundColor DarkGray
}

# ---------------------------------------------------------------------------
# REPORT HELPERS
# ---------------------------------------------------------------------------

function Write-ReportHeader {
    param([string]$ReportFile, [string]$stepLabel, [string]$description)
    $bar = "=" * 72
    @(
        $bar
        "  $stepLabel  -  $description"
        $bar
        "Run ID    : $RunId"
        "Started   : $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))"
        "Host      : $env:COMPUTERNAME"
        "User      : $env:USERNAME"
        $bar
        ""
    ) | Set-Content -Path $ReportFile -Encoding UTF8
}

function Write-ReportFooter {
    param([string]$ReportFile, [string]$status, [timespan]$elapsed, [string]$metrics)
    $bar = "=" * 72
    @(
        ""
        $bar
        "Status    : $status"
        "Metrics   : $metrics"
        "Duration  : $($elapsed.ToString('hh\:mm\:ss\.fff'))"
        "Finished  : $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))"
        "Report    : $ReportFile"
        $bar
    ) | Add-Content -Path $ReportFile -Encoding UTF8
}

# ---------------------------------------------------------------------------
# NATIVE COMMAND RUNNER
#
# Streams stdout to console + report file in real time.
# Stderr is captured via 2>&1 and shown in DarkYellow (warnings).
# $ErrorActionPreference is temporarily set to "Continue" so that ErrorRecord
# objects from native stderr do NOT trigger the global "Stop" behavior in
# PowerShell 5.1 and abort the pipeline before $LASTEXITCODE is captured.
# ---------------------------------------------------------------------------

function Invoke-NativeCommand {
    param(
        [string]   $ReportFile,
        [string]   $FailMessage,
        [string]   $Exe,
        [string[]] $Arguments,
        [ref]      $Capture      # optional: receives all output lines as string[]
    )

    $cmdLine = "$Exe $($Arguments -join ' ')"
    Write-Info "> $cmdLine" "DarkGray"
    Add-Content -Path $ReportFile -Value "CMD: $cmdLine" -Encoding UTF8
    Add-Content -Path $ReportFile -Value "" -Encoding UTF8

    $lines = [System.Collections.Generic.List[string]]::new()
    $code  = 0

    # Temporarily lower EAP so PS 5.1 does not throw on native stderr (ErrorRecord)
    # when it flows through the 2>&1 pipeline. We check $LASTEXITCODE ourselves.
    $savedEAP = $ErrorActionPreference
    $ErrorActionPreference = "Continue"

    # StreamWriter keeps the file handle open for the entire pipeline, avoiding the
    # "file in use by another process" IOException that repeated Add-Content calls
    # cause when a fast-output command (e.g. docker build) hammers the pipeline.
    $writer = [System.IO.StreamWriter]::new(
        $ReportFile, $true, [System.Text.Encoding]::UTF8)
    try {
        & $Exe @Arguments 2>&1 | ForEach-Object {
            if ($_ -is [System.Management.Automation.ErrorRecord]) {
                # stderr line from native command - show as warning
                $line = $_.ToString()
                Write-Host "    $line" -ForegroundColor DarkYellow
            } else {
                $line = "$_"
                Write-Host "    $line"
            }
            $lines.Add($line)
            $writer.WriteLine($line)
        }
        $code = $LASTEXITCODE
    }
    finally {
        $writer.Close()
        $ErrorActionPreference = $savedEAP
    }

    Add-Content -Path $ReportFile -Value "" -Encoding UTF8
    if ($Capture) { $Capture.Value = $lines.ToArray() }

    if ($code -ne 0) {
        $msg = "$FailMessage (exit code $code)"
        Add-Content -Path $ReportFile -Value "FAILURE: $msg" -Encoding UTF8
        throw $msg
    }
}

# ---------------------------------------------------------------------------
# STEP WRAPPER
# ---------------------------------------------------------------------------

function Invoke-Step {
    param(
        [string]      $Name,
        [string]      $Description,
        [scriptblock] $Body
    )

    $script:StepCounter++
    $padded     = $script:StepCounter.ToString().PadLeft(2, '0')
    $stepLabel  = "STEP $padded"
    $reportFile = Join-Path $ReportsDir "$padded-$Name.txt"
    $startTime  = Get-Date

    New-Item -ItemType Directory -Force -Path $ReportsDir | Out-Null
    Write-Banner "$stepLabel  -  $Description"
    Write-ReportHeader -ReportFile $reportFile -stepLabel $stepLabel -description $Description

    $script:LastMetrics = ""
    $status  = "PASS"
    $errText = ""

    try {
        & $Body $reportFile
    }
    catch {
        $status  = "FAIL"
        $errText = $_.ToString()
        Add-Content -Path $reportFile -Value "`nFAILURE: $errText" -Encoding UTF8
    }

    $elapsed = (Get-Date) - $startTime
    $metrics = $script:LastMetrics
    Write-ReportFooter -ReportFile $reportFile -status $status -elapsed $elapsed -metrics $metrics

    $script:StepResults.Add(@{
        Pad        = $padded
        Name       = $Name
        Desc       = $Description
        Status     = $status
        Duration   = $elapsed
        Metrics    = $metrics
        ReportFile = $reportFile
        Error      = $errText
    })

    Write-Divider
    if ($status -eq "PASS") {
        $dur = $elapsed.ToString('mm\:ss')
        Write-Host "  PASS  $Description  [$dur]" -ForegroundColor Green
        if ($metrics) { Write-Host "        $metrics" -ForegroundColor DarkGreen }
    }
    else {
        Write-Host "  FAIL  $Description" -ForegroundColor Red
        Write-Host "  Error   : $errText" -ForegroundColor Red
        Write-Host "  Report  : $reportFile" -ForegroundColor Yellow
        throw "Step '$Name' failed. Check $reportFile for details."
    }
}

# ---------------------------------------------------------------------------
# SUMMARY PRINTER
# ---------------------------------------------------------------------------

function Write-Summary {
    $summaryFile  = Join-Path $ReportsDir "00-summary.txt"
    $totalElapsed = (Get-Date) - $script:RunStartTime
    # @(...) forces array so .Count reflects item count, not hashtable key count
    $passed       = @($script:StepResults | Where-Object { $_.Status -eq "PASS" }).Count
    $failed       = @($script:StepResults | Where-Object { $_.Status -eq "FAIL" }).Count
    $overall      = if ($failed -eq 0) { "ALL STEPS PASSED" } else { "SOME STEPS FAILED" }
    $overallColor = if ($failed -eq 0) { "Green" } else { "Red" }

    $bar = "=" * 72

    # ---- console table ----
    Write-Banner "EXECUTION SUMMARY" -color $overallColor
    Write-Host ""
    Write-Host ("  " + "Step".PadRight(22) + "Status  " + "Duration  " + "Metrics") -ForegroundColor DarkCyan
    Write-Host ("  " + ("-" * 68)) -ForegroundColor DarkGray

    foreach ($r in $script:StepResults) {
        $sym   = if ($r.Status -eq "PASS") { "PASS" } else { "FAIL" }
        $color = if ($r.Status -eq "PASS") { "Green" } else { "Red" }
        $dur   = $r.Duration.ToString('mm\:ss')
        $label = ("$($r.Pad) $($r.Name)").PadRight(22)
        $met   = if ($r.Metrics) { $r.Metrics } else { "-" }
        Write-Host ("  " + $label + "$sym    $dur    $met") -ForegroundColor $color
        if ($r.Status -eq "FAIL") {
            Write-Host ("  " + " " * 22 + "    Error: $($r.Error)") -ForegroundColor DarkRed
        }
    }

    Write-Host ("  " + ("-" * 68)) -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "  Run ID    : $RunId" -ForegroundColor White
    Write-Host "  Started   : $($script:RunStartTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor White
    Write-Host "  Finished  : $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor White
    Write-Host "  Duration  : $($totalElapsed.ToString('hh\:mm\:ss'))" -ForegroundColor White
    Write-Host "  Steps     : $($script:StepResults.Count) total  /  $passed passed  /  $failed failed" -ForegroundColor White
    Write-Host "  Reports   : $ReportsDir" -ForegroundColor White
    Write-Host ""
    Write-Host ("  " + $bar) -ForegroundColor $overallColor
    Write-Host "  OVERALL: $overall" -ForegroundColor $overallColor
    Write-Host ("  " + $bar) -ForegroundColor $overallColor

    # ---- file report ----
    $fileLines = @(
        $bar
        "  SETUP SUMMARY  -  Ambev Developer Evaluation"
        $bar
        "Run ID    : $RunId"
        "Started   : $($script:RunStartTime.ToString('yyyy-MM-dd HH:mm:ss'))"
        "Finished  : $((Get-Date).ToString('yyyy-MM-dd HH:mm:ss'))"
        "Duration  : $($totalElapsed.ToString('hh\:mm\:ss'))"
        "Steps     : $($script:StepResults.Count) total, $passed passed, $failed failed"
        $bar
        ""
        ("  " + "Step".PadRight(22) + "Status  Duration  Metrics")
        ("  " + "-" * 68)
    )
    foreach ($r in $script:StepResults) {
        $sym = if ($r.Status -eq "PASS") { "PASS" } else { "FAIL" }
        $dur = $r.Duration.ToString('mm\:ss')
        $met = if ($r.Metrics) { $r.Metrics } else { "-" }
        $fileLines += "  " + ("$($r.Pad) $($r.Name)").PadRight(22) + "$sym    $dur    $met"
        if ($r.Status -eq "FAIL") {
            $fileLines += "  " + " " * 22 + "Error: $($r.Error)"
        }
    }
    $fileLines += @("", $bar, "  OVERALL: $overall", $bar)
    $fileLines | Set-Content -Path $summaryFile -Encoding UTF8
}

# ---------------------------------------------------------------------------
# TEST METRICS HELPER
# Parses dotnet test output and returns "X passed, Y failed, Z skipped".
# dotnet test (xunit) emits:
#   "  Passed!  - Failed:  0, Passed: 228, Skipped:  0, Total: 228, Duration: ..."
# Note: "Failed:" appears before "Passed:" in that line.
# ---------------------------------------------------------------------------

function Get-TestMetrics {
    param([string[]]$output)

    $passed = 0; $failed = 0; $skipped = 0

    # Format A (xunit inline): "Passed! - Failed: 0, Passed: 228, Skipped: 0, Total: 228, Duration:..."
    $summaryLine = $output |
        Where-Object { "$_" -match 'Failed:\s*\d+' -and "$_" -match 'Passed:\s*\d+' -and "$_" -match 'Total:\s*\d+' } |
        Select-Object -Last 1

    if ($summaryLine) {
        if ("$summaryLine" -match 'Passed:\s*(\d+)')  { $passed  = [int]$Matches[1] }
        if ("$summaryLine" -match 'Failed:\s*(\d+)')  { $failed  = [int]$Matches[1] }
        if ("$summaryLine" -match 'Skipped:\s*(\d+)') { $skipped = [int]$Matches[1] }
        return "$passed passed, $failed failed, $skipped skipped"
    }

    # Format B (VSTest multiline) -- dotnet test default output:
    #   Total tests: 240
    #        Passed: 240
    #   (Failed/Skipped lines absent when count is zero)
    $totalLine = $output | Where-Object { "$_" -match '^\s*Total tests:\s*\d+' } | Select-Object -Last 1
    if ($totalLine) {
        $passLine = $output | Where-Object { "$_" -match '^\s*Passed:\s*\d+\s*$' } | Select-Object -Last 1
        $failLine = $output | Where-Object { "$_" -match '^\s*Failed:\s*\d+\s*$' } | Select-Object -Last 1
        $skipLine = $output | Where-Object { "$_" -match '^\s*Skipped:\s*\d+\s*$' } | Select-Object -Last 1
        if ($passLine -and ("$passLine" -match '(\d+)')) { $passed  = [int]$Matches[1] }
        if ($failLine -and ("$failLine" -match '(\d+)')) { $failed  = [int]$Matches[1] }
        if ($skipLine -and ("$skipLine" -match '(\d+)')) { $skipped = [int]$Matches[1] }
        return "$passed passed, $failed failed, $skipped skipped"
    }

    return "see report for details"
}

# ---------------------------------------------------------------------------
# STEP 1 - PREREQUISITES
# ---------------------------------------------------------------------------

function Invoke-Prerequisites {
    Invoke-Step -Name "Prerequisites" -Description "Check required tools" -Body {
        param($rf)

        Write-Info "Checking .NET SDK..."
        Add-Content $rf "--- .NET SDK ---" -Encoding UTF8
        $dotnetVerLines = $null
        Invoke-NativeCommand -ReportFile $rf -FailMessage "dotnet SDK not found" `
            -Exe "dotnet" -Arguments @("--version") -Capture ([ref]$dotnetVerLines)
        $dotnetVer = "$($dotnetVerLines | Select-Object -First 1)".Trim()
        Write-Ok ".NET SDK $dotnetVer"

        $sdkList = & dotnet --list-sdks 2>&1
        $sdkList | ForEach-Object { Add-Content $rf "$_" -Encoding UTF8 }
        $sdk8 = $sdkList | Where-Object { "$_" -match "^8\." }
        if (-not $sdk8) { throw ".NET 8 SDK is required but not found. Install from https://dot.net" }
        Add-Content $rf "" -Encoding UTF8

        Write-Info "Checking Docker..."
        Add-Content $rf "--- Docker ---" -Encoding UTF8
        $dockerVerLines = $null
        Invoke-NativeCommand -ReportFile $rf -FailMessage "Docker CLI not found" `
            -Exe "docker" -Arguments @("--version") -Capture ([ref]$dockerVerLines)
        $dockerVer = "$($dockerVerLines | Select-Object -First 1)".Trim()
        Write-Ok "Docker: $dockerVer"

        Write-Info "Checking Docker daemon..."
        Invoke-NativeCommand -ReportFile $rf -FailMessage "Docker daemon not running - start Docker Desktop." `
            -Exe "docker" -Arguments @("info", "--format", "Server Version: {{.ServerVersion}}")
        Write-Ok "Docker daemon is running"
        Add-Content $rf "" -Encoding UTF8

        Write-Info "Checking Python..."
        Add-Content $rf "--- Python (required for ApiTest step) ---" -Encoding UTF8
        $pyExe = $null; $pyVer = ""
        foreach ($candidate in @("python", "python3", "py")) {
            try {
                $ver = & $candidate --version 2>&1
                if ($LASTEXITCODE -eq 0) { $pyExe = $candidate; $pyVer = "$ver".Trim(); break }
            } catch { }
        }
        if ($pyExe) {
            Add-Content $rf "Python executable : $pyExe" -Encoding UTF8
            Add-Content $rf "Version           : $pyVer" -Encoding UTF8
            $env:PYTHON_EXE = $pyExe
            Write-Ok "Python: $pyVer"
        } else {
            Add-Content $rf "WARNING: Python not found. ApiTest step will be skipped." -Encoding UTF8
            Write-Warn "Python not found - ApiTest step will be skipped."
            $env:PYTHON_EXE = ""
            $pyVer = "not found"
        }
        Add-Content $rf "" -Encoding UTF8

        Write-Info "Checking reportgenerator..."
        Add-Content $rf "--- dotnet-reportgenerator-globaltool ---" -Encoding UTF8
        $rgRaw = & reportgenerator --version 2>&1
        $rgVer = ""
        if ($LASTEXITCODE -ne 0) {
            Write-Info "Installing reportgenerator..." "Yellow"
            Add-Content $rf "Not installed - installing now..." -Encoding UTF8
            Invoke-NativeCommand -ReportFile $rf -FailMessage "Failed to install reportgenerator" `
                -Exe "dotnet" -Arguments @("tool", "install", "-g", "dotnet-reportgenerator-globaltool")
            $rgRaw = & reportgenerator --version 2>&1
            $rgVer = "$($rgRaw | Where-Object { "$_" -match '^\d+\.' } | Select-Object -First 1)".Trim()
            if (-not $rgVer) { $rgVer = "installed" }
            Write-Ok "reportgenerator installed: $rgVer"
        } else {
            # reportgenerator logs timestamp lines; find the version (starts with digit)
            $rgVer = "$($rgRaw | Where-Object { "$_" -match '^\d+\.' } | Select-Object -First 1)".Trim()
            if (-not $rgVer) { $rgVer = "installed" }
            Add-Content $rf "reportgenerator version: $rgVer" -Encoding UTF8
            Write-Ok "reportgenerator: $rgVer"
        }
        Add-Content $rf "" -Encoding UTF8

        $script:LastMetrics = "dotnet $dotnetVer | Python: $pyVer | reportgenerator: $rgVer"
    }
}

# ---------------------------------------------------------------------------
# STEP 2 - RESTORE
# ---------------------------------------------------------------------------

function Invoke-Restore {
    Invoke-Step -Name "Restore" -Description "dotnet restore (NuGet packages)" -Body {
        param($rf)
        Write-Info "Restoring NuGet packages..."
        $out = $null
        Invoke-NativeCommand -ReportFile $rf -FailMessage "dotnet restore failed" `
            -Exe "dotnet" -Arguments @("restore", $Sln, "--verbosity", "minimal") `
            -Capture ([ref]$out)

        $restored = $out | Where-Object { "$_" -match "packages restored|Restored" } | Select-Object -Last 1
        if ($restored) {
            Write-Ok "$restored".Trim()
            $script:LastMetrics = "$restored".Trim()
        } else {
            Write-Ok "Packages restored"
            $script:LastMetrics = "Packages restored"
        }
    }
}

# ---------------------------------------------------------------------------
# STEP 3 - BUILD
# ---------------------------------------------------------------------------

function Invoke-Build {
    Invoke-Step -Name "Build" -Description "dotnet build --configuration Release" -Body {
        param($rf)
        Write-Info "Compiling solution in Release configuration..."
        $out = $null
        Invoke-NativeCommand -ReportFile $rf -FailMessage "dotnet build failed" `
            -Exe "dotnet" -Arguments @(
                "build", $Sln,
                "--configuration", "Release",
                "--no-restore",
                "--verbosity", "minimal"
            ) -Capture ([ref]$out)

        $warnings = 0; $errors = 0
        $wLine = $out | Where-Object { "$_" -match '\d+ Warning\(s\)' } | Select-Object -Last 1
        $eLine = $out | Where-Object { "$_" -match '\d+ Error\(s\)' }   | Select-Object -Last 1
        if ($wLine -match '(\d+) Warning') { $warnings = [int]$Matches[1] }
        if ($eLine -match '(\d+) Error')   { $errors   = [int]$Matches[1] }

        Write-Ok "Build succeeded - $errors error(s), $warnings warning(s)"
        $script:LastMetrics = "$errors error(s), $warnings warning(s)"
    }
}

# ---------------------------------------------------------------------------
# STEP 4 - UNIT TESTS
# ---------------------------------------------------------------------------

function Invoke-UnitTests {
    Invoke-Step -Name "UnitTests" -Description "Unit tests (no Docker)" -Body {
        param($rf)
        Write-Info "Running unit tests..."
        $out = $null
        Invoke-NativeCommand -ReportFile $rf -FailMessage "Unit tests failed" `
            -Exe "dotnet" -Arguments @(
                "test", $TestUnit,
                "--configuration", "Release",
                "--no-build",
                "--verbosity", "normal",
                "--logger", "console;verbosity=normal"
            ) -Capture ([ref]$out)

        $metrics = Get-TestMetrics $out
        Write-Ok "Tests complete: $metrics"
        $script:LastMetrics = $metrics
    }
}

# ---------------------------------------------------------------------------
# STEP 5 - COVERAGE
# ---------------------------------------------------------------------------

function Invoke-Coverage {
    Invoke-Step -Name "Coverage" -Description "Coverage report (Domain + Application)" -Body {
        param($rf)
        Write-Info "Running tests with Coverlet instrumentation..."
        Add-Content $rf "Delegating to coverage-report.ps1 ..." -Encoding UTF8
        Add-Content $rf "" -Encoding UTF8

        # Run the coverage script with the same EAP trick so its stderr is safe
        $savedEAP = $ErrorActionPreference
        $ErrorActionPreference = "Continue"
        $coverCode = 0
        try {
            & powershell -File $CoveragePs1 2>&1 | ForEach-Object {
                $line = if ($_ -is [System.Management.Automation.ErrorRecord]) {
                    Write-Host "    $($_.ToString())" -ForegroundColor DarkYellow
                    $_.ToString()
                } else {
                    Write-Host "    $_"
                    "$_"
                }
                Add-Content $rf $line -Encoding UTF8
            }
            $coverCode = $LASTEXITCODE
        }
        finally {
            $ErrorActionPreference = $savedEAP
        }
        if ($coverCode -ne 0) { throw "coverage-report.ps1 exited with code $coverCode" }

        $summaryTxt = Join-Path $Root "coverage-report\Summary.txt"
        $branchPct = "n/a"; $linePct = "n/a"
        if (Test-Path $summaryTxt) {
            Add-Content $rf "" -Encoding UTF8
            Add-Content $rf "--- Coverage Summary ---" -Encoding UTF8
            $summaryLines = Get-Content $summaryTxt
            $summaryLines | Add-Content $rf -Encoding UTF8

            $bLine = $summaryLines | Where-Object { $_ -match "Branch coverage" } | Select-Object -Last 1
            $lLine = $summaryLines | Where-Object { $_ -match "Line coverage"   } | Select-Object -Last 1
            if ($bLine -match '([\d.]+)%') { $branchPct = "$($Matches[1])%" }
            if ($lLine -match '([\d.]+)%') { $linePct   = "$($Matches[1])%" }

            Write-Info "Coverage: Branch $branchPct  |  Line $linePct" "Cyan"
        }

        $script:LastMetrics = "Branch: $branchPct  |  Line: $linePct"
        $htmlReport = Join-Path $Root "coverage-report\index.html"
        if (Test-Path $htmlReport) { Write-Ok "HTML report: $htmlReport" }
    }
}

# ---------------------------------------------------------------------------
# STEP 6 - INTEGRATION TESTS
# ---------------------------------------------------------------------------

function Invoke-IntegrationTests {
    Invoke-Step -Name "IntegrationTests" -Description "Integration tests (Testcontainers + PostgreSQL)" -Body {
        param($rf)
        Write-Info "Starting integration tests (Testcontainers spins up PostgreSQL)..."
        Write-Info "First run may take 1-3 min for Docker image pull." "DarkYellow"
        $out = $null
        Invoke-NativeCommand -ReportFile $rf -FailMessage "Integration tests failed" `
            -Exe "dotnet" -Arguments @(
                "test", $TestInt,
                "--configuration", "Release",
                "--no-build",
                "--verbosity", "normal",
                "--logger", "console;verbosity=normal"
            ) -Capture ([ref]$out)

        $metrics = Get-TestMetrics $out
        Write-Ok "Tests complete: $metrics"
        $script:LastMetrics = $metrics
    }
}

# ---------------------------------------------------------------------------
# STEP 7 - FUNCTIONAL TESTS
# ---------------------------------------------------------------------------

function Invoke-FunctionalTests {
    Invoke-Step -Name "FunctionalTests" -Description "Functional tests" -Body {
        param($rf)
        Write-Info "Running functional tests..."
        $out = $null
        Invoke-NativeCommand -ReportFile $rf -FailMessage "Functional tests failed" `
            -Exe "dotnet" -Arguments @(
                "test", $TestFunc,
                "--configuration", "Release",
                "--no-build",
                "--verbosity", "normal",
                "--logger", "console;verbosity=normal"
            ) -Capture ([ref]$out)

        $metrics = Get-TestMetrics $out
        Write-Ok "Tests complete: $metrics"
        $script:LastMetrics = $metrics
    }
}

# ---------------------------------------------------------------------------
# STEP 8 - DOCKER BUILD
# ---------------------------------------------------------------------------

function Invoke-DockerBuild {
    Invoke-Step -Name "DockerBuild" -Description "docker-compose build (WebApi image)" -Body {
        param($rf)
        Write-Info "Building Docker image (this may take several minutes)..." "DarkYellow"
        Push-Location $Root
        try {
            Invoke-NativeCommand -ReportFile $rf -FailMessage "docker-compose build failed" `
                -Exe "docker-compose" -Arguments @("build", "--no-cache")
        } finally {
            Pop-Location
        }
        Write-Ok "Docker image built"
        $script:LastMetrics = "Image built successfully"
    }
}

# ---------------------------------------------------------------------------
# STEP 9 - DOCKER UP
# ---------------------------------------------------------------------------

function Invoke-DockerUp {
    Invoke-Step -Name "DockerUp" -Description "docker-compose up -d + wait for API" -Body {
        param($rf)
        Write-Info "Starting containers (WebApi, PostgreSQL, MongoDB, RabbitMQ)..."
        Push-Location $Root
        try {
            Invoke-NativeCommand -ReportFile $rf -FailMessage "docker-compose up failed" `
                -Exe "docker-compose" -Arguments @("up", "-d", "--remove-orphans")
        } finally {
            Pop-Location
        }

        Add-Content $rf "" -Encoding UTF8
        Add-Content $rf "--- Waiting for API on $ApiBaseUrl (max ${ApiReadyMaxSecs}s) ---" -Encoding UTF8
        Write-Info "Waiting for API to respond on $ApiBaseUrl ..." "DarkYellow"

        $pollUrl  = "$ApiBaseUrl/api/users"
        $deadline = (Get-Date).AddSeconds($ApiReadyMaxSecs)
        $ready    = $false
        $attempt  = 0

        while ((Get-Date) -lt $deadline) {
            $attempt++
            $savedEAP = $ErrorActionPreference
            $ErrorActionPreference = "Continue"
            try {
                $resp = Invoke-WebRequest -Uri $pollUrl -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
                if ($resp.StatusCode -gt 0) { $ready = $true }
            }
            catch [System.Net.WebException] {
                if ($_.Exception.Response -and
                    [int]$_.Exception.Response.StatusCode -eq 401) {
                    $ready = $true
                }
            }
            catch { }
            finally {
                $ErrorActionPreference = $savedEAP
            }

            if ($ready) { break }

            $remaining = [int]($deadline - (Get-Date)).TotalSeconds
            Write-Info "API not ready yet - attempt $attempt ($remaining s remaining)..." "DarkGray"
            Start-Sleep -Milliseconds $ApiReadyPollMs
        }

        if (-not $ready) {
            Add-Content $rf "`n--- Container logs (webapi) ---" -Encoding UTF8
            $savedEAP2 = $ErrorActionPreference
            $ErrorActionPreference = "Continue"
            try {
                & docker-compose -f (Join-Path $Root "docker-compose.yml") `
                                 -f (Join-Path $Root "docker-compose.override.yml") `
                    logs --tail 50 ambev.developerevaluation.webapi 2>&1 |
                    ForEach-Object { Add-Content $rf "$_" -Encoding UTF8 }
            }
            finally { $ErrorActionPreference = $savedEAP2 }
            throw "API did not become ready within ${ApiReadyMaxSecs}s."
        }

        $waitSecs = $attempt * ($ApiReadyPollMs / 1000)
        Write-Ok "API is ready at $ApiBaseUrl (after ~${waitSecs}s)"
        Add-Content $rf "API is ready at $ApiBaseUrl" -Encoding UTF8

        Write-Info "Container status:"
        Add-Content $rf "" -Encoding UTF8
        Add-Content $rf "--- Container status ---" -Encoding UTF8
        Push-Location $Root
        try {
            Invoke-NativeCommand -ReportFile $rf -FailMessage "docker-compose ps failed" `
                -Exe "docker-compose" -Arguments @("ps")
        } finally {
            Pop-Location
        }

        $script:LastMetrics = "API ready at $ApiBaseUrl in ~${waitSecs}s"
    }
}

# ---------------------------------------------------------------------------
# STEP 10 - API TEST
# ---------------------------------------------------------------------------

function Invoke-ApiTest {
    Invoke-Step -Name "ApiTest" -Description "End-to-end API test (scripts/test_api.py)" -Body {
        param($rf)

        $pyExe = $env:PYTHON_EXE
        if ([string]::IsNullOrWhiteSpace($pyExe)) {
            foreach ($candidate in @("python", "python3", "py")) {
                try {
                    $null = & $candidate --version 2>&1
                    if ($LASTEXITCODE -eq 0) { $pyExe = $candidate; break }
                } catch { }
            }
        }
        if ([string]::IsNullOrWhiteSpace($pyExe)) {
            throw "Python interpreter not found. Install Python 3.x and ensure it is in PATH."
        }
        if (-not (Test-Path $TestApiPy)) {
            throw "test_api.py not found at: $TestApiPy"
        }

        Add-Content $rf "Python  : $pyExe" -Encoding UTF8
        Add-Content $rf "Script  : $TestApiPy" -Encoding UTF8
        Add-Content $rf "Target  : $ApiBaseUrl" -Encoding UTF8
        Add-Content $rf "" -Encoding UTF8

        Write-Info "Running $pyExe $TestApiPy against $ApiBaseUrl ..." "DarkYellow"
        $output = & $pyExe $TestApiPy 2>&1

        foreach ($line in $output) {
            $lineStr = "$line"
            Write-Host "    $lineStr"
            Add-Content $rf $lineStr -Encoding UTF8
        }

        $summaryLine = $output | Where-Object { "$_" -match 'Total:.*Failed:\s*(\d+)' } |
                       Select-Object -Last 1
        $failCount = 0; $totalCount = 0; $passCount = 0
        if ($summaryLine) {
            if ("$summaryLine" -match 'Failed:\s*(\d+)')  { $failCount  = [int]$Matches[1] }
            if ("$summaryLine" -match 'Total:\s*(\d+)')   { $totalCount = [int]$Matches[1] }
            if ("$summaryLine" -match 'Passed:\s*(\d+)')  { $passCount  = [int]$Matches[1] }
        }

        Add-Content $rf "" -Encoding UTF8
        $script:LastMetrics = "Total: $totalCount | Passed: $passCount | Failed: $failCount"

        if ($failCount -gt 0) {
            throw "test_api.py reported $failCount failed test(s). See report for details."
        }
        Write-Ok "All API tests passed ($passCount/$totalCount)"
        Add-Content $rf "All API tests passed." -Encoding UTF8
    }
}

# ---------------------------------------------------------------------------
# STEP 11 - DOCKER DOWN
# ---------------------------------------------------------------------------

function Invoke-DockerDown {
    Invoke-Step -Name "DockerDown" -Description "docker-compose down -v (cleanup)" -Body {
        param($rf)
        Write-Info "Stopping and removing containers, networks and volumes..."
        Push-Location $Root
        try {
            Invoke-NativeCommand -ReportFile $rf -FailMessage "docker-compose down failed" `
                -Exe "docker-compose" -Arguments @("down", "-v", "--remove-orphans")
        } finally {
            Pop-Location
        }
        Write-Ok "All containers removed"
        $script:LastMetrics = "Containers, networks and volumes removed"
    }
}

# ---------------------------------------------------------------------------
# STEP ROUTER - maps step name to function
# ---------------------------------------------------------------------------

function Invoke-SingleStep([string]$stepName) {
    switch ($stepName) {
        "Prerequisites"    { Invoke-Prerequisites    }
        "Restore"          { Invoke-Restore          }
        "Build"            { Invoke-Build            }
        "UnitTests"        { Invoke-UnitTests        }
        "Coverage"         { Invoke-Coverage         }
        "IntegrationTests" { Invoke-IntegrationTests }
        "FunctionalTests"  { Invoke-FunctionalTests  }
        "DockerBuild"      { Invoke-DockerBuild      }
        "DockerUp"         { Invoke-DockerUp         }
        "ApiTest"          { Invoke-ApiTest          }
        "DockerDown"       { Invoke-DockerDown       }
    }
}

# ---------------------------------------------------------------------------
# FULL RUN (sequential, all steps)
# ---------------------------------------------------------------------------

function Invoke-FullRun([int]$startIdx = 0) {
    for ($i = $startIdx; $i -lt $script:StepNames.Count; $i++) {
        $sn = $script:StepNames[$i]

        if ($sn -eq "DockerDown" -and $KeepContainers) {
            Write-Host ""
            Write-Warn "-KeepContainers: skipping DockerDown"
            Write-Host "  API      : $ApiBaseUrl" -ForegroundColor Yellow
            Write-Host "  Teardown : docker-compose down -v" -ForegroundColor Yellow
            break
        }

        Invoke-SingleStep $sn
    }
}

# ---------------------------------------------------------------------------
# ENTRY POINT
# ---------------------------------------------------------------------------

New-Item -ItemType Directory -Force -Path $ReportsDir | Out-Null

Write-Banner "Ambev Developer Evaluation  -  Environment Setup v2.1" -color "White"
Write-Host "  Run ID   : $RunId" -ForegroundColor Gray
Write-Host "  Reports  : $ReportsDir" -ForegroundColor Gray

$modeLabel = if ($Step -eq "All") {
    "Full run (11 steps)"
} elseif ($From) {
    $startNum = [Array]::IndexOf($script:StepNames, $Step) + 1
    "From step $startNum ($Step) to end"
} else {
    $stepNum = [Array]::IndexOf($script:StepNames, $Step) + 1
    "Single step $stepNum ($Step)"
}
Write-Host "  Mode     : $modeLabel" -ForegroundColor Gray
Write-Host ""

try {
    if ($Step -eq "All") {
        Invoke-FullRun -startIdx 0
    }
    elseif ($From) {
        $startIdx = [Array]::IndexOf($script:StepNames, $Step)
        Invoke-FullRun -startIdx $startIdx
    }
    else {
        Invoke-SingleStep $Step
    }

    Write-Summary
    Write-Host ""
    try { Read-Host "  Press ENTER to close" } catch { }
    exit 0
}
catch {
    Write-Host ""
    Write-Host "  ABORTED: $($_.ToString())" -ForegroundColor Red
    Write-Summary
    Write-Host ""
    try { Read-Host "  Press ENTER to close" } catch { }
    exit 1
}
