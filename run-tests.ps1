# ═══════════════════════════════════════════════════════════════════════
#  Sqworgram Unit Tests Runner - PowerShell версия
#  Интерактивный запуск тестов
# ═══════════════════════════════════════════════════════════════════════

param(
    [string]$Test = "all"
)

$ErrorActionPreference = "Stop"

# Проверка .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK найден: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK не установлен" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       Sqworgram Unit Tests Runner (PowerShell)     ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

function Show-Menu {
    Clear-Host
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host "  ВЫБЕРИТЕ ДЕЙСТВИЕ:" -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  1. Запустить ВСЕ тесты"
    Write-Host "  2. TranslationServiceTests (7 тестов)"
    Write-Host "  3. ImageUploaderTests (7 тестов)"
    Write-Host "  4. DatabaseHelperTests (14 тестов)"
    Write-Host "  5. ThemeManagerTests (8 тестов)"
    Write-Host "  6. AvatarConverterTests (10 тестов)"
    Write-Host ""
    Write-Host "  7. Тесты с ПОКРЫТИЕМ кода"
    Write-Host "  8. Генерировать HTML отчёт"
    Write-Host ""
    Write-Host "  0. Выход"
    Write-Host ""
    Write-Host "Введите номер: " -ForegroundColor Green -NoNewline
    $choice = Read-Host
    return $choice
}

function Run-Tests {
    param(
        [string]$TestName,
        [string]$Filter
    )
    
    Write-Host ""
    Write-Host "▶ Запуск $TestName..." -ForegroundColor Cyan
    Write-Host "─────────────────────────────────────────────" -ForegroundColor Cyan
    Write-Host ""
    
    if ($Filter -eq "") {
        dotnet test --verbosity normal
    } else {
        dotnet test --filter "ClassName=$Filter" --verbosity normal
    }
    
    $exitCode = $LASTEXITCODE
    
    Write-Host ""
    Write-Host "─────────────────────────────────────────────" -ForegroundColor Cyan
    
    if ($exitCode -eq 0) {
        Write-Host "✅ $TestName - УСПЕШНО!" -ForegroundColor Green
    } else {
        Write-Host "❌ $TestName - ОШИБКА!" -ForegroundColor Red
    }
    
    Read-Host "Нажмите Enter для продолжения"
}

function Run-Tests-With-Coverage {
    Write-Host ""
    Write-Host "▶ Запуск тестов с ПОКРЫТИЕМ кода..." -ForegroundColor Cyan
    Write-Host "─────────────────────────────────────────────" -ForegroundColor Cyan
    Write-Host ""
    
    dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover --verbosity normal
    
    $exitCode = $LASTEXITCODE
    
    Write-Host ""
    Write-Host "─────────────────────────────────────────────" -ForegroundColor Cyan
    
    if ($exitCode -eq 0) {
        Write-Host "✅ Тесты с покрытием - УСПЕШНО!" -ForegroundColor Green
        Write-Host "📊 Файл: Sqworgram.UnitTests/bin/Debug/net472/coverage.opencover.xml"
    } else {
        Write-Host "❌ Тесты с покрытием - ОШИБКА!" -ForegroundColor Red
    }
    
    Read-Host "Нажмите Enter для продолжения"
}

function Generate-Coverage-Report {
    Write-Host ""
    Write-Host "▶ Генерирование HTML отчёта покрытия..." -ForegroundColor Cyan
    Write-Host "─────────────────────────────────────────────" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "1️⃣  Запуск тестов для сбора покрытия..." -ForegroundColor Yellow
    dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover -q
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Ошибка при запуске тестов!" -ForegroundColor Red
        Read-Host "Нажмите Enter для продолжения"
        return
    }
    
    Write-Host "✅ Тесты выполнены" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "2️⃣  Проверка ReportGenerator..." -ForegroundColor Yellow
    
    $reportGenInstalled = dotnet tool list -g | Select-String "reportgenerator"
    
    if (-not $reportGenInstalled) {
        Write-Host "⚠️  ReportGenerator не установлен. Установка..." -ForegroundColor Yellow
        dotnet tool install -g dotnet-reportgenerator-globaltool
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Ошибка установки ReportGenerator!" -ForegroundColor Red
            Read-Host "Нажмите Enter для продолжения"
            return
        }
        
        Write-Host "✅ ReportGenerator установлен" -ForegroundColor Green
    } else {
        Write-Host "✅ ReportGenerator уже установлен" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "3️⃣  Генерирование отчёта..." -ForegroundColor Yellow
    
    reportgenerator -reports:"**/coverage.opencover.xml" -targetdir:"./coverage-report" -reporttypes:Html
    
    Write-Host ""
    Write-Host "─────────────────────────────────────────────" -ForegroundColor Cyan
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ HTML отчёт - УСПЕШНО СГЕНЕРИРОВАН!" -ForegroundColor Green
        Write-Host "📊 Путь: ./coverage-report/index.html"
        Write-Host ""
        
        $openReport = Read-Host "🌐 Открыть отчёт в браузере? (y/n)"
        if ($openReport -eq "y" -or $openReport -eq "Y") {
            $reportPath = Join-Path (Get-Location) "coverage-report\index.html"
            if (Test-Path $reportPath) {
                Start-Process $reportPath
                Write-Host "✅ Отчёт открыт в браузере" -ForegroundColor Green
            } else {
                Write-Host "❌ Файл отчёта не найден: $reportPath" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "❌ Ошибка при генерировании отчёта!" -ForegroundColor Red
    }
    
    Read-Host "Нажмите Enter для продолжения"
}

# Основной цикл
while ($true) {
    $choice = Show-Menu
    
    switch ($choice) {
        "1" { Run-Tests "все тесты" "" }
        "2" { Run-Tests "TranslationServiceTests" "TranslationServiceTests" }
        "3" { Run-Tests "ImageUploaderTests" "ImageUploaderTests" }
        "4" { Run-Tests "DatabaseHelperTests" "DatabaseHelperTests" }
        "5" { Run-Tests "ThemeManagerTests" "ThemeManagerTests" }
        "6" { Run-Tests "AvatarUrlToImageSourceConverterTests" "AvatarUrlToImageSourceConverterTests" }
        "7" { Run-Tests-With-Coverage }
        "8" { Generate-Coverage-Report }
        "0" {
            Write-Host ""
            Write-Host "👋 До свидания!" -ForegroundColor Yellow
            exit 0
        }
        default {
            Write-Host "❌ Неверный выбор. Попробуйте снова." -ForegroundColor Red
            Read-Host "Нажмите Enter для продолжения"
        }
    }
}
