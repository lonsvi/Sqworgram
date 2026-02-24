using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Sqworgram.TestRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║  Sqworgram Unit Tests Runner               ║");
            Console.WriteLine("║  Интерактивный запуск тестов               ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            while (true)
            {
                ShowMenu();
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("═══════════════════════════════════════════════");
            Console.WriteLine("  ВЫБЕРИТЕ ДЕЙСТВИЕ:");
            Console.WriteLine("═══════════════════════════════════════════════");
            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine("  1. Запустить ВСЕ тесты");
            Console.WriteLine("  2. Запустить тесты TranslationService");
            Console.WriteLine("  3. Запустить тесты ImageUploader");
            Console.WriteLine("  4. Запустить тесты DatabaseHelper");
            Console.WriteLine("  5. Запустить тесты ThemeManager");
            Console.WriteLine("  6. Запустить тесты AvatarConverter");
            Console.WriteLine();
            Console.WriteLine("  7. Запустить тесты с ПОКРЫТИЕМ кода");
            Console.WriteLine("  8. Генерировать HTML отчёт покрытия");
            Console.WriteLine();
            Console.WriteLine("  0. Выход");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  Введите номер: ");
            Console.ResetColor();

            string choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    RunTests("все тесты", "");
                    break;
                case "2":
                    RunTests("TranslationServiceTests", "TranslationServiceTests");
                    break;
                case "3":
                    RunTests("ImageUploaderTests", "ImageUploaderTests");
                    break;
                case "4":
                    RunTests("DatabaseHelperTests", "DatabaseHelperTests");
                    break;
                case "5":
                    RunTests("ThemeManagerTests", "ThemeManagerTests");
                    break;
                case "6":
                    RunTests("AvatarUrlToImageSourceConverterTests", "AvatarUrlToImageSourceConverterTests");
                    break;
                case "7":
                    RunTestsWithCoverage();
                    break;
                case "8":
                    GenerateCoverageReport();
                    break;
                case "0":
                    ExitApp();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Неверный выбор. Попробуйте снова.");
                    Console.ResetColor();
                    break;
            }
        }

        static void RunTests(string testName, string filter)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"▶ Запуск {testName}...");
            Console.WriteLine("─────────────────────────────────────────────");
            Console.ResetColor();
            Console.WriteLine();

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = filter != "" ? $"test Sqworgram.UnitTests --filter \"ClassName={filter}\" --verbosity detailed" : "test Sqworgram.UnitTests --verbosity detailed",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = false
                };

                var process = Process.Start(processInfo);
                process.WaitForExit();

                Console.WriteLine();
                Console.WriteLine("─────────────────────────────────────────────");

                if (process.ExitCode == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✅ {testName} - ВСЕ ТЕСТЫ ПРОШЛИ УСПЕШНО!");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ {testName} - ОШИБКА! (см. результаты выше)");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Ошибка при запуске: {ex.Message}");
                Console.ResetColor();
            }

            PressAnyKeyToContinue();
        }

        static void RunTestsWithCoverage()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("▶ Запуск тестов с ПОКРЫТИЕМ кода...");
            Console.WriteLine("─────────────────────────────────────────────");
            Console.ResetColor();
            Console.WriteLine();

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "test Sqworgram.UnitTests /p:CollectCoverage=true /p:CoverageFormat=opencover --verbosity detailed",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = false
                };

                var process = Process.Start(processInfo);
                process.WaitForExit();

                Console.WriteLine();
                Console.WriteLine("─────────────────────────────────────────────");

                if (process.ExitCode == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ Тесты с покрытием выполнены успешно!");
                    Console.ResetColor();
                    Console.WriteLine();
                    
                    // Поиск файла покрытия
                    var coverageFile = FindCoverageFile();
                    if (coverageFile != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"📊 Файл покрытия найден:");
                        Console.WriteLine($"   {coverageFile}");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Тесты с покрытием - ОШИБКА!");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Ошибка при запуске: {ex.Message}");
                Console.ResetColor();
            }

            PressAnyKeyToContinue();
        }

        static void GenerateCoverageReport()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("▶ Генерирование HTML отчёта покрытия...");
            Console.WriteLine("─────────────────────────────────────────────");
            Console.ResetColor();
            Console.WriteLine();

            try
            {
                // Сначала запустим тесты с покрытием
                Console.WriteLine("1️⃣  Запуск тестов для сбора покрытия...");
                var testProcess = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "test Sqworgram.UnitTests /p:CollectCoverage=true /p:CoverageFormat=opencover",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                var process = Process.Start(testProcess);
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Ошибка при запуске тестов!");
                    Console.ResetColor();
                    PressAnyKeyToContinue();
                    return;
                }

                Console.WriteLine("✅ Тесты выполнены");
                Console.WriteLine();
                Console.WriteLine("2️⃣  Проверка ReportGenerator...");

                // Проверим, установлен ли ReportGenerator
                var checkProcess = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "tool list -g",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                var check = Process.Start(checkProcess);
                var output = check.StandardOutput.ReadToEnd();
                check.WaitForExit();

                if (!output.Contains("reportgenerator"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️  ReportGenerator не установлен. Установка...");
                    Console.ResetColor();

                    var installProcess = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = "tool install -g dotnet-reportgenerator-globaltool",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    var install = Process.Start(installProcess);
                    install.WaitForExit();

                    if (install.ExitCode != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("❌ Ошибка установки ReportGenerator!");
                        Console.ResetColor();
                        PressAnyKeyToContinue();
                        return;
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ ReportGenerator установлен");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("✅ ReportGenerator уже установлен");
                }

                Console.WriteLine();
                Console.WriteLine("3️⃣  Генерирование отчёта...");

                // Поиск файла покрытия
                var coverageFilePath = FindCoverageFile();
                if (coverageFilePath == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Файл покрытия не найден!");
                    Console.WriteLine("   Проверьте что файл .NET Framework тестов скомпилирован правильно.");
                    Console.ResetColor();
                    PressAnyKeyToContinue();
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"📊 Используем файл: {coverageFilePath}");
                Console.ResetColor();
                Console.WriteLine();

                var reportProcess = new ProcessStartInfo
                {
                    FileName = "reportgenerator",
                    Arguments = $"-reports:\"{coverageFilePath}\" -targetdir:\"./coverage-report\" -reporttypes:Html",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = false
                };

                var report = Process.Start(reportProcess);
                report.WaitForExit();

                Console.WriteLine();
                Console.WriteLine("─────────────────────────────────────────────");

                if (report.ExitCode == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ HTML отчёт - УСПЕШНО СГЕНЕРИРОВАН!");
                    Console.ResetColor();
                    Console.WriteLine();
                    Console.WriteLine("📊 Путь: ./coverage-report/index.html");
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("🌐 Открыть отчёт в браузере? (y/n): ");
                    Console.ResetColor();
                    if (Console.ReadLine()?.ToLower() == "y")
                    {
                        OpenHtmlReport();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Ошибка при генерировании отчёта!");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
                Console.ResetColor();
            }

            PressAnyKeyToContinue();
        }

        static void OpenHtmlReport()
        {
            try
            {
                string reportPath = Path.Combine(Directory.GetCurrentDirectory(), "coverage-report", "index.html");
                
                if (File.Exists(reportPath))
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = reportPath,
                        UseShellExecute = true
                    };

                    Process.Start(processInfo);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ Отчёт открыт в браузере");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ Файл не найден: {reportPath}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Ошибка при открытии отчёта: {ex.Message}");
                Console.ResetColor();
            }
        }

        static string FindCoverageFile()
        {
            try
            {
                string currentDir = Directory.GetCurrentDirectory();
                Console.WriteLine("   🔍 Поиск файла покрытия:");
                Console.WriteLine($"   Текущая папка: {currentDir}");
                Console.WriteLine();

                // Поиск 1: Debug/net472
                var debugPath = Path.Combine(currentDir, "Sqworgram.UnitTests", "bin", "Debug", "net472", "coverage.opencover.xml");
                Console.WriteLine($"   1️⃣  Проверка: Debug/net472");
                Console.WriteLine($"       {debugPath}");
                if (File.Exists(debugPath))
                {
                    Console.WriteLine("       ✅ Найден!");
                    Console.WriteLine();
                    return debugPath;
                }
                Console.WriteLine("       ❌ Не найден");
                Console.WriteLine();

                // Поиск 2: Release/net472
                var releasePath = Path.Combine(currentDir, "Sqworgram.UnitTests", "bin", "Release", "net472", "coverage.opencover.xml");
                Console.WriteLine($"   2️⃣  Проверка: Release/net472");
                Console.WriteLine($"       {releasePath}");
                if (File.Exists(releasePath))
                {
                    Console.WriteLine("       ✅ Найден!");
                    Console.WriteLine();
                    return releasePath;
                }
                Console.WriteLine("       ❌ Не найден");
                Console.WriteLine();

                // Поиск 3: Debug (без сублапок)
                var debugRoot = Path.Combine(currentDir, "Sqworgram.UnitTests", "bin", "Debug", "coverage.opencover.xml");
                Console.WriteLine($"   3️⃣  Проверка: Debug (корень)");
                Console.WriteLine($"       {debugRoot}");
                if (File.Exists(debugRoot))
                {
                    Console.WriteLine("       ✅ Найден!");
                    Console.WriteLine();
                    return debugRoot;
                }
                Console.WriteLine("       ❌ Не найден");
                Console.WriteLine();

                // Поиск 4: Рекурсивный поиск
                Console.WriteLine($"   4️⃣  Рекурсивный поиск по всем папкам...");
                var files = Directory.GetFiles(
                    currentDir,
                    "coverage.opencover.xml",
                    SearchOption.AllDirectories
                );

                if (files.Length > 0)
                {
                    Console.WriteLine($"       ✅ Найдено файлов: {files.Length}");
                    foreach (var f in files)
                    {
                        var info = new FileInfo(f);
                        Console.WriteLine($"          • {f}");
                        Console.WriteLine($"            Размер: {info.Length} байт, Изменен: {info.LastWriteTime}");
                    }
                    Console.WriteLine();
                    var newest = files.OrderByDescending(f => new FileInfo(f).LastWriteTime).FirstOrDefault();
                    Console.WriteLine($"       ✅ Используем самый новый файл");
                    return newest;
                }
                
                Console.WriteLine("       ❌ Не найдено файлов");
                Console.WriteLine();

                return null;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"       ❌ Ошибка поиска: {ex.Message}");
                Console.ResetColor();
                return null;
            }
        }

        static void ExitApp()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("👋 До свидания!");
            Console.ResetColor();
            Environment.Exit(0);
        }

        static void PressAnyKeyToContinue()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Нажмите любую клавишу для продолжения...");
            Console.ResetColor();
            Console.ReadKey();
            Console.Clear();
        }
    }
}
