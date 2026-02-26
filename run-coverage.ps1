#!/usr/bin/env pwsh
# Run tests with code coverage and generate HTML report

param(
    [string]$ReportGenerator = "reportgenerator"
)

$ErrorActionPreference = "Stop"

Write-Host "🧪 Sqworgram Test Coverage Report Generator" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Set paths
$testProjectPath = ".\Sqworgram.FrameworkTests"
$coveragePath = "$testProjectPath\coverage"
$reportPath = "$testProjectPath\CoverageReport"

# Clean old coverage
if (Test-Path $coveragePath) {
    Write-Host "🧹 Cleaning old coverage data..." -ForegroundColor Yellow
    Remove-Item -Path $coveragePath -Recurse -Force
}

if (Test-Path $reportPath) {
    Remove-Item -Path $reportPath -Recurse -Force
}

# Rebuild solution
Write-Host "`n🔨 Rebuilding solution..." -ForegroundColor Yellow
dotnet clean --configuration Debug 2>&1 | Out-Null
dotnet build --configuration Debug --verbosity minimal 2>&1 | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build successful" -ForegroundColor Green

# Run tests with coverage
Write-Host "`n🚀 Running tests with code coverage..." -ForegroundColor Yellow
Write-Host "   This may take a few minutes..." -ForegroundColor Gray

dotnet test "$testProjectPath\Sqworgram.FrameworkTests.csproj" `
    --no-build `
    --configuration Debug `
    --logger "console;verbosity=minimal" `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=opencover `
    /p:CoverletOutput="$coveragePath/" `
    /p:Exclude="[*]*.xaml.cs,[*]App.xaml.cs" `
    2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  Some tests may have failed, but continuing with coverage report..." -ForegroundColor Yellow
}

# Check if coverage file was generated
$coverageFile = Get-ChildItem -Path $coveragePath -Filter "coverage.opencover.xml" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1

if (-not $coverageFile) {
    Write-Host "❌ Coverage file not generated!" -ForegroundColor Red
    Write-Host "   Available files in coverage directory:" -ForegroundColor Gray
    Get-ChildItem -Path $coveragePath -Recurse | ForEach-Object { Write-Host "   - $_" -ForegroundColor Gray }
    exit 1
}

Write-Host "✅ Coverage data collected: $($coverageFile.FullName)" -ForegroundColor Green

# Generate HTML report
Write-Host "`n📊 Generating HTML coverage report..." -ForegroundColor Yellow

$reportCmd = @(
    "`"$($coverageFile.FullName)`"",
    "-targetdir:`"$reportPath`"",
    "-reporttypes:Html_Dark",
    "-verbosity:Error"
)

Write-Host "   Command: $ReportGenerator $($reportCmd -join ' ')" -ForegroundColor Gray

& $ReportGenerator ($reportCmd -join [Environment]::NewLine)

if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  ReportGenerator not found. Installing globally..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
    
    & $ReportGenerator ($reportCmd -join [Environment]::NewLine)
}

# Open report
if (Test-Path "$reportPath\index.html") {
    Write-Host "`n✅ Coverage report generated successfully!" -ForegroundColor Green
    Write-Host "   📁 Report location: $reportPath" -ForegroundColor Cyan
    Write-Host "   🌐 Opening report..." -ForegroundColor Cyan
    
    Start-Process "$reportPath\index.html"
}
else {
    Write-Host "❌ Report file not found!" -ForegroundColor Red
}

Write-Host "`n✅ Test coverage analysis complete!" -ForegroundColor Green
