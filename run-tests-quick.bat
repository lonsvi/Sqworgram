@echo off
chcp 65001 > nul
setlocal enabledelayedexpansion

echo ============================================
echo  🧪 Sqworgram Test Runner
echo ============================================
echo.

:: Проверка наличия .NET SDK
dotnet --version >nul 2>&1
if errorlevel 1 (
    color 0C
    echo ❌ .NET SDK не установлен!
    pause
    exit /b 1
)

:: Главное меню
:menu
color 0A
cls
echo ============================================
echo  🧪 Sqworgram Test Runner Menu
echo ============================================
echo.
echo  1️⃣  TranslationServiceTests
echo  2️⃣  ImageUploaderTests  
echo  3️⃣  DatabaseHelperTests
echo  4️⃣  ThemeManagerTests
echo  5️⃣  AvatarUrlToImageSourceConverterTests
echo  6️⃣  Запустить все тесты
echo  7️⃣  Запустить с покрытием кода
echo  8️⃣  Генерировать HTML отчёт
echo  0️⃣  Выход
echo.
echo ============================================
set /p choice="Выберите опцию (0-8): "

if "%choice%"=="0" goto exit_app
if "%choice%"=="1" goto test1
if "%choice%"=="2" goto test2
if "%choice%"=="3" goto test3
if "%choice%"=="4" goto test4
if "%choice%"=="5" goto test5
if "%choice%"=="6" goto test_all
if "%choice%"=="7" goto test_coverage
if "%choice%"=="8" goto report_html

echo ❌ Неверный выбор!
pause
goto menu

:test1
echo.
echo 🧪 TranslationServiceTests...
cd /d "%~dp0"
dotnet test Sqworgram.UnitTests --filter "ClassName=Sqworgram.UnitTests.TranslationServiceTests" --verbosity detailed
pause
goto menu

:test2
echo.
echo 🧪 ImageUploaderTests...
cd /d "%~dp0"
dotnet test Sqworgram.UnitTests --filter "ClassName=Sqworgram.UnitTests.ImageUploaderTests" --verbosity detailed
pause
goto menu

:test3
echo.
echo 🧪 DatabaseHelperTests...
cd /d "%~dp0"
dotnet test Sqworgram.UnitTests --filter "ClassName=Sqworgram.UnitTests.DatabaseHelperTests" --verbosity detailed
pause
goto menu

:test4
echo.
echo 🧪 ThemeManagerTests...
cd /d "%~dp0"
dotnet test Sqworgram.UnitTests --filter "ClassName=Sqworgram.UnitTests.ThemeManagerTests" --verbosity detailed
pause
goto menu

:test5
echo.
echo 🧪 AvatarUrlToImageSourceConverterTests...
cd /d "%~dp0"
dotnet test Sqworgram.UnitTests --filter "ClassName=Sqworgram.UnitTests.AvatarUrlToImageSourceConverterTests" --verbosity detailed
pause
goto menu

:test_all
echo.
echo 🧪 Запуск всех тестов...
cd /d "%~dp0"
dotnet test Sqworgram.UnitTests --verbosity detailed
pause
goto menu

:test_coverage
echo.
echo 📊 Запуск тестов с покрытием...
cd /d "%~dp0"
dotnet test Sqworgram.UnitTests /p:CollectCoverage=true /p:CoverageFormat=opencover --verbosity detailed
echo.
echo ✅ Тесты выполнены!
echo 📁 Файл покрытия: Sqworgram.UnitTests\bin\Debug\net472\coverage.opencover.xml
pause
goto menu

:report_html
echo.
echo 📊 Генерирование HTML отчёта...
cd /d "%~dp0"

echo 🔍 Запуск тестов с покрытием...
dotnet test Sqworgram.UnitTests /p:CollectCoverage=true /p:CoverageFormat=opencover --verbosity quiet

echo 📦 Проверка ReportGenerator...
dotnet tool list -g | findstr /i "reportgenerator" >nul 2>&1
if errorlevel 1 (
    echo 📥 Установка ReportGenerator...
    dotnet tool install -g dotnet-reportgenerator-globaltool
)

echo 📄 Генерирование отчёта...
if exist "Sqworgram.UnitTests\bin\Debug\net472\coverage.opencover.xml" (
    reportgenerator -reports:"Sqworgram.UnitTests\bin\Debug\net472\coverage.opencover.xml" -targetdir:".\coverage-report" -reporttypes:Html
    echo.
    echo ✅ Отчёт сгенерирован в coverage-report\index.html
    echo.
    set /p open="Открыть отчёт в браузере? (y/n): "
    if /i "!open!"=="y" (
        start .\coverage-report\index.html
    )
) else (
    echo ❌ Файл покрытия не найден!
    echo 📁 Попробуйте: Sqworgram.UnitTests\bin\Release\net472\coverage.opencover.xml
)
pause
goto menu

:exit_app
color 0E
echo.
echo 👋 До свидания!
exit /b 0
