@echo off
REM ═════════════════════════════════════════════════════════════════════
REM  Sqworgram Unit Tests Runner - Быстрый запуск
REM  Автоматический запуск TestRunner консоли
REM ═════════════════════════════════════════════════════════════════════

setlocal enabledelayedexpansion

REM Определяем текущую директорию
set SCRIPT_DIR=%~dp0
cd /d "%SCRIPT_DIR%"

REM Проверяем, установлен ли .NET SDK
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    color 0C
    echo.
    echo ╔════════════════════════════════════════════════════════╗
    echo ║          ОШИБКА: .NET SDK не установлен!               ║
    echo ║     Пожалуйста, установите .NET SDK 6.0 или выше       ║
    echo ║      https://dotnet.microsoft.com/download/            ║
    echo ╚════════════════════════════════════════════════════════╝
    echo.
    pause
    exit /b 1
)

REM Проверяем наличие проекта TestRunner
if not exist "Sqworgram.TestRunner\Sqworgram.TestRunner.csproj" (
    color 0C
    echo.
    echo ╔════════════════════════════════════════════════════════╗
    echo ║          ОШИБКА: Проект TestRunner не найден!          ║
    echo ║  Убедитесь, что вы запускаете скрипт из корня решения  ║
    echo ╚════════════════════════════════════════════════════════╝
    echo.
    pause
    exit /b 1
)

color 0B
cls

echo.
echo ╔════════════════════════════════════════════════════════════╗
echo ║       Sqworgram Unit Tests Interactive Runner               ║
echo ║       Загрузка интерактивной консоли тестов...             ║
echo ╚════════════════════════════════════════════════════════════╝
echo.

REM Запускаем TestRunner
dotnet run --project Sqworgram.TestRunner\Sqworgram.TestRunner.csproj

exit /b 0
