<#
Runs tests for both services, collects coverage and generates HTML report.
Usage (PowerShell):
  ./scripts/run-tests-and-report.ps1

It will:
 - load `.env` files from both services (if present)
 - run `dotnet test` for the two test projects with XPlat coverage
 - install ReportGenerator locally (tool manifest) if missing
 - generate HTML coverage report at testresults/coverage-report
#>

Set-StrictMode -Version Latest

function Load-EnvFile($path) {
    if (-not (Test-Path $path)) { return }
    Get-Content $path | ForEach-Object {
        $line = $_.Trim()
        if ($line -eq '' -or $line.StartsWith('#')) { return }
        $parts = $line -split '=', 2
        if ($parts.Length -ne 2) { return }
        $name = $parts[0].Trim()
        $value = $parts[1].Trim().Trim('"')
    # convert ':' to '__' for env name mapping
    $envName = $name -replace ':','__'
    Write-Host "Setting env $envName"
    ${env:$envName} = $value
    }
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
# repo root is parent of scripts directory
$root = Resolve-Path (Join-Path $scriptDir '..')

Write-Host "Loading .env files (if any)"
Load-EnvFile (Join-Path $root 'Authentication_Service\.env')
Load-EnvFile (Join-Path $root 'Service_Management_Service\.env')

Write-Host "Ensuring tool manifest and ReportGenerator are installed locally"
if (-not (Test-Path (Join-Path $root '.config\dotnet-tools.json'))) {
    Push-Location $root
    dotnet new tool-manifest | Out-Null
    Pop-Location
}

# Install reportgenerator as a local tool if missing
$tools = dotnet tool list --local 2>$null | Out-String
if ($tools -notmatch 'reportgenerator') {
    Write-Host "Installing reportgenerator local tool..."
    Push-Location $root
    dotnet tool install dotnet-reportgenerator-globaltool --local | Out-Null
    Pop-Location
} else {
    Write-Host "reportgenerator local tool already installed"
}

$tests = @(
    @{proj='Service_Management_Service.Tests\Service_Management_Service.Tests.csproj'; results='testresults\service_management'},
    @{proj='Authentication_Service.Tests\Authentication_Service.Tests.csproj'; results='testresults\auth'}
)

foreach ($t in $tests) {
    $projPath = Join-Path $root $t.proj
    $resultsDir = Join-Path $root $t.results
    Write-Host "Running tests for $($t.proj) -> $resultsDir"
    if (-not (Test-Path $resultsDir)) { New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null }

    dotnet test $projPath --verbosity minimal --logger "trx;LogFileName=Tests.trx" --results-directory $resultsDir --collect:"XPlat Code Coverage"
}

Write-Host "Collecting cobertura files"
$reports = Get-ChildItem -Path (Join-Path $root 'testresults') -Recurse -Filter 'coverage.cobertura.xml' | ForEach-Object { $_.FullName }
if (-not $reports) {
    Write-Host "No coverage reports found. Exiting with code 1" -ForegroundColor Yellow
    exit 1
}

$reportList = $reports -join ';'
$target = Join-Path $root 'testresults\coverage-report'
if (-not (Test-Path $target)) { New-Item -ItemType Directory -Force -Path $target | Out-Null }

Write-Host "Generating HTML coverage report to $target"
& dotnet tool run reportgenerator -reports:$reportList -targetdir:$target -reporttypes:HtmlSummary
if ($LASTEXITCODE -ne 0) {
    Write-Host "reportgenerator failed" -ForegroundColor Red
    exit 1
}

Write-Host "Coverage report generated: $target\index.htm"
Write-Host "Done"
