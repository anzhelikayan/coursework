using System;
using System.IO;
using System.Diagnostics;
using BusStationTicketSystem.Models;

namespace BusStationTicketSystem.Services
{
    public class LatexTicketGenerator
    {
        public string GenerateTicket(Ticket ticket)
        {
            // Перевірка на null
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket), "Квиток не може бути null");
            }
            
            // Перевірка та ініціалізація Route, якщо він null
            if (ticket.Route == null)
            {
                ticket.Route = new Route
                {
                    Id = 0,
                    Departure = "Не вказано",
                    Arrival = "Не вказано",
                    Distance = 0,
                    BasePrice = 0,
                    DepartureTime = TimeSpan.Zero
                };
            }
            
            // Перевірка та ініціалізація Passenger, якщо він null
            if (ticket.Passenger == null)
            {
                ticket.Passenger = new Passenger
                {
                    LastName = "Не вказано",
                    FirstName = "",
                    MiddleName = "",
                    Phone = "",
                    Document = ""
                };
            }
            
            try
            {
                // Генеруємо LaTeX код
                var latexCode = GenerateLatexCode(ticket);
                
                // Створення папки для експортованих квитків
                string projectFolder;
                try
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var baseDirInfo = new DirectoryInfo(baseDir);
                    
                    if (baseDirInfo.Name == "net8.0-windows" && 
                        (baseDirInfo.Parent?.Name == "Release" || baseDirInfo.Parent?.Name == "Debug"))
                    {
                        projectFolder = baseDirInfo.Parent?.Parent?.FullName ?? baseDir;
                    }
                    else
                    {
                        var currentDir = baseDirInfo;
                        while (currentDir != null && 
                               !File.Exists(Path.Combine(currentDir.FullName, "BusStationTicketSystem.csproj")))
                        {
                            currentDir = currentDir.Parent;
                        }
                        
                        if (currentDir != null && File.Exists(Path.Combine(currentDir.FullName, "BusStationTicketSystem.csproj")))
                        {
                            projectFolder = currentDir.FullName;
                        }
                        else
                        {
                            projectFolder = baseDir;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Помилка пошуку папки проекту: {ex.Message}");
                    projectFolder = AppDomain.CurrentDomain.BaseDirectory;
                }
                
                var exportFolder = Path.Combine(projectFolder, "ExportedTickets");
                try
                {
                    if (!Directory.Exists(exportFolder))
                    {
                        Directory.CreateDirectory(exportFolder);
                        System.Diagnostics.Debug.WriteLine($"Створено папку для експорту: {exportFolder}");
                    }
                }
                catch (Exception dirEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Помилка створення папки: {dirEx.Message}");
                    exportFolder = Path.Combine(projectFolder, "ExportedTickets");
                }
                
                // Збереження LaTeX файлу
                var latexFileName = $"Ticket_{ticket.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.tex";
                var latexFilePath = Path.Combine(exportFolder, latexFileName);
                
                System.Diagnostics.Debug.WriteLine($"Спроба збереження LaTeX файлу: {latexFilePath}");
                
                // Зберігаємо LaTeX код
                File.WriteAllText(latexFilePath, latexCode, System.Text.Encoding.UTF8);
                
                // Компілюємо LaTeX в PDF
                var pdfFilePath = CompileLatexToPdf(latexFilePath, exportFolder);
                
                if (!string.IsNullOrEmpty(pdfFilePath) && File.Exists(pdfFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"PDF успішно згенеровано: {pdfFilePath}");
                    return pdfFilePath;
                }
                else
                {
                    // Якщо компіляція не вдалася, повертаємо LaTeX файл
                    System.Diagnostics.Debug.WriteLine($"Компіляція LaTeX не вдалася, повертаю LaTeX файл: {latexFilePath}");
                    return latexFilePath;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"КРИТИЧНА ПОМИЛКА при генерації LaTeX: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Внутрішня помилка: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                }
                
                var errorMessage = $"Помилка генерації LaTeX: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nДеталі: {ex.InnerException.Message}";
                }
                throw new InvalidOperationException(errorMessage, ex);
            }
        }
        
        private string GenerateLatexCode(Ticket ticket)
        {
            var latex = new System.Text.StringBuilder();
            
            // Заголовок документа
            latex.AppendLine(@"\documentclass[a4paper,10pt]{article}");
            latex.AppendLine(@"\usepackage[utf8]{inputenc}");
            latex.AppendLine(@"\usepackage[ukrainian]{babel}");
            latex.AppendLine(@"\usepackage{geometry}");
            latex.AppendLine(@"\geometry{margin=2cm}");
            latex.AppendLine(@"\usepackage{graphicx}");
            latex.AppendLine(@"\usepackage{booktabs}");
            latex.AppendLine(@"\usepackage{array}");
            latex.AppendLine(@"\usepackage{xcolor}");
            latex.AppendLine(@"\usepackage{fancyhdr}");
            latex.AppendLine(@"\pagestyle{empty}");
            latex.AppendLine();
            latex.AppendLine(@"\begin{document}");
            latex.AppendLine();
            
            // Заголовок
            latex.AppendLine(@"\begin{center}");
            latex.AppendLine(@"\Large\textbf{АВТОВОКЗАЛ}\\[0.5cm]");
            latex.AppendLine(@"\large\textbf{ПОСАДОЧНИЙ ДОКУМЕНТ}\\[0.3cm]");
            latex.AppendLine(@"\rule{\textwidth}{0.5pt}\\[0.5cm]");
            latex.AppendLine(@"\end{center}");
            latex.AppendLine();
            
            // Маршрут
            string departure = EscapeLatex(ticket.Route.Departure ?? "Не вказано");
            string arrival = EscapeLatex(ticket.Route.Arrival ?? "Не вказано");
            latex.AppendLine(@"\textbf{МАРШРУТ:}\\[0.2cm]");
            latex.AppendLine($@"\hspace{{1cm}}{departure} $\rightarrow$ {arrival}\\[0.5cm]");
            latex.AppendLine();
            
            // Дата та час
            string dateStr;
            if (ticket.Date == default(DateTime) || ticket.Date == DateTime.MinValue)
            {
                dateStr = DateTime.Now.ToString("dd.MM.yyyy");
            }
            else
            {
                dateStr = ticket.Date.ToString("dd.MM.yyyy");
            }
            
            string timeStr;
            if (ticket.Route.DepartureTime == default(TimeSpan))
            {
                timeStr = "Не вказано";
            }
            else
            {
                timeStr = ticket.Route.DepartureTime.ToString(@"hh\:mm");
            }
            
            latex.AppendLine(@"\textbf{ДАТА ТА ЧАС ВІДПРАВЛЕННЯ:}\\[0.2cm]");
            latex.AppendLine($@"\hspace{{1cm}}Дата: {EscapeLatex(dateStr)}\\[0.1cm]");
            latex.AppendLine($@"\hspace{{1cm}}Час: {EscapeLatex(timeStr)}\\[0.5cm]");
            latex.AppendLine();
            
            // Номер автобуса та місце
            int busNumber = 100 + (Math.Abs(ticket.Id) % 50);
            int seatNumber = 1 + (Math.Abs(ticket.Id) % 50);
            latex.AppendLine(@"\textbf{ТРАНСПОРТ:}\\[0.2cm]");
            latex.AppendLine($@"\hspace{{1cm}}Автобус: {busNumber}\\[0.1cm]");
            latex.AppendLine($@"\hspace{{1cm}}Місце: {seatNumber}\\[0.5cm]");
            latex.AppendLine();
            
            latex.AppendLine(@"\rule{\textwidth}{0.3pt}\\[0.3cm]");
            latex.AppendLine();
            
            // Дані пасажира
            string lastName = EscapeLatex(ticket.Passenger.LastName ?? "");
            string firstName = EscapeLatex(ticket.Passenger.FirstName ?? "");
            string fullName = $"{lastName} {firstName}".Trim();
            
            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = "Не вказано";
            }
            else
            {
                fullName = fullName.ToUpper();
                if (!string.IsNullOrWhiteSpace(ticket.Passenger.MiddleName))
                {
                    fullName += $" {EscapeLatex(ticket.Passenger.MiddleName)}";
                }
            }
            
            latex.AppendLine(@"\textbf{ДАНІ ПАСАЖИРА:}\\[0.2cm]");
            latex.AppendLine($@"\hspace{{1cm}}Прізвище, Ім'я: \textbf{{{fullName}}}\\[0.2cm]");
            
            if (!string.IsNullOrWhiteSpace(ticket.Passenger.Document))
            {
                latex.AppendLine($@"\hspace{{1cm}}Документ: {EscapeLatex(ticket.Passenger.Document.ToUpper())}\\[0.3cm]");
            }
            latex.AppendLine();
            
            latex.AppendLine(@"\rule{\textwidth}{0.3pt}\\[0.3cm]");
            latex.AppendLine();
            
            // UID квитка
            int ticketId = Math.Abs(ticket.Id);
            string uid = $"{ticketId:D4}-{DateTime.Now.Millisecond:D4}-{ticketId % 10000:D4}-{ticketId % 10000:D4}";
            latex.AppendLine(@"\textbf{UID КВИТКА:}\\[0.2cm]");
            latex.AppendLine($@"\hspace{{1cm}}\texttt{{{uid}}}\\[0.3cm]");
            latex.AppendLine();
            
            latex.AppendLine(@"\rule{\textwidth}{0.3pt}\\[0.3cm]");
            latex.AppendLine();
            
            // Фіскальна інформація
            latex.AppendLine(@"\textbf{ФІСКАЛЬНА ІНФОРМАЦІЯ:}\\[0.2cm]");
            latex.AppendLine($@"\hspace{{1cm}}Номер квитка: {ticket.Id}\\[0.1cm]");
            
            string ticketType = (ticket.TicketType ?? "Regular") switch
            {
                "Discount" => "Пільговий",
                "Child" => "Дитячий",
                _ => "Звичайний"
            };
            latex.AppendLine($@"\hspace{{1cm}}Тип: {EscapeLatex(ticketType)}\\[0.1cm]");
            
            decimal price = ticket.Price;
            if (price < 0) price = 0;
            latex.AppendLine($@"\hspace{{1cm}}Вартість: \textbf{{{price:C}}}\\[0.1cm]");
            latex.AppendLine($@"\hspace{{1cm}}Дата оформлення: {EscapeLatex(DateTime.Now.ToString("dd.MM.yyyy HH:mm"))}\\[0.1cm]");
            
            string status = (ticket.Status ?? "Active") switch
            {
                "Active" => "Активний",
                "Cancelled" => "Скасований",
                "Used" => "Використаний",
                _ => EscapeLatex(ticket.Status ?? "Активний")
            };
            latex.AppendLine($@"\hspace{{1cm}}Статус: {status}\\[0.3cm]");
            latex.AppendLine();
            
            latex.AppendLine(@"\rule{\textwidth}{0.5pt}\\[0.5cm]");
            latex.AppendLine();
            latex.AppendLine(@"\begin{center}");
            latex.AppendLine(@"\textit{Дякуємо за вибір наших послуг!}");
            latex.AppendLine(@"\end{center}");
            latex.AppendLine();
            
            latex.AppendLine(@"\end{document}");
            
            return latex.ToString();
        }
        
        private string EscapeLatex(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            
            return text
                .Replace(@"\", @"\textbackslash{}")
                .Replace("{", @"\{")
                .Replace("}", @"\}")
                .Replace("$", @"\$")
                .Replace("&", @"\&")
                .Replace("#", @"\#")
                .Replace("^", @"\textasciicircum{}")
                .Replace("_", @"\_")
                .Replace("%", @"\%")
                .Replace("~", @"\textasciitilde{}");
        }
        
        private string? CompileLatexToPdf(string latexFilePath, string outputFolder)
        {
            try
            {
                // Перевіряємо наявність LaTeX компілятора
                string? latexCompiler = FindLatexCompiler();
                
                if (string.IsNullOrEmpty(latexCompiler))
                {
                    System.Diagnostics.Debug.WriteLine("LaTeX компілятор не знайдено. Пропускаю компіляцію.");
                    return null;
                }
                
                System.Diagnostics.Debug.WriteLine($"Використовую LaTeX компілятор: {latexCompiler}");
                
                var processInfo = new ProcessStartInfo
                {
                    FileName = latexCompiler,
                    Arguments = $"-interaction=nonstopmode -output-directory=\"{outputFolder}\" \"{latexFilePath}\"",
                    WorkingDirectory = outputFolder,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                
                using (var process = Process.Start(processInfo))
                {
                    if (process == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Не вдалося запустити LaTeX компілятор");
                        return null;
                    }
                    
                    process.WaitForExit(30000); // Чекаємо до 30 секунд
                    
                    if (process.ExitCode == 0)
                    {
                        // Знаходимо згенерований PDF файл
                        var pdfFileName = Path.GetFileNameWithoutExtension(latexFilePath) + ".pdf";
                        var pdfFilePath = Path.Combine(outputFolder, pdfFileName);
                        
                        if (File.Exists(pdfFilePath))
                        {
                            System.Diagnostics.Debug.WriteLine($"PDF успішно скомпільовано: {pdfFilePath}");
                            return pdfFilePath;
                        }
                    }
                    else
                    {
                        var errorOutput = process.StandardError.ReadToEnd();
                        System.Diagnostics.Debug.WriteLine($"Помилка компіляції LaTeX (код: {process.ExitCode}): {errorOutput}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка компіляції LaTeX: {ex.Message}");
            }
            
            return null;
        }
        
        private string? FindLatexCompiler()
        {
            // Список можливих шляхів до LaTeX компіляторів
            var possiblePaths = new[]
            {
                @"C:\Program Files\MiKTeX\miktex\bin\x64\pdflatex.exe",
                @"C:\Program Files\MiKTeX 2.9\miktex\bin\x64\pdflatex.exe",
                @"C:\Program Files (x86)\MiKTeX\miktex\bin\pdflatex.exe",
                @"C:\Program Files (x86)\MiKTeX 2.9\miktex\bin\pdflatex.exe",
                @"C:\texlive\2024\bin\win32\pdflatex.exe",
                @"C:\texlive\2023\bin\win32\pdflatex.exe",
                @"C:\texlive\2022\bin\win32\pdflatex.exe",
                @"pdflatex.exe" // Якщо в PATH
            };
            
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
                
                // Спробуємо знайти через PATH
                if (path == "pdflatex.exe")
                {
                    try
                    {
                        var processInfo = new ProcessStartInfo
                        {
                            FileName = "where",
                            Arguments = "pdflatex",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        };
                        
                        using (var process = Process.Start(processInfo))
                        {
                            if (process != null)
                            {
                                var output = process.StandardOutput.ReadToEnd();
                                process.WaitForExit();
                                
                                if (!string.IsNullOrWhiteSpace(output))
                                {
                                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (lines.Length > 0 && File.Exists(lines[0].Trim()))
                                    {
                                        return lines[0].Trim();
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Ігноруємо помилки пошуку
                    }
                }
            }
            
            return null;
        }
    }
}

