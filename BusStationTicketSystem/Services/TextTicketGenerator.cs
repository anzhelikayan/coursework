using System;
using System.IO;
using BusStationTicketSystem.Models;

namespace BusStationTicketSystem.Services
{
    public class TextTicketGenerator
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
                // Формуємо текст квитка
                var ticketText = GenerateTicketText(ticket);
                
                // Створення папки для експортованих квитків
                string projectFolder;
                try
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var baseDirInfo = new DirectoryInfo(baseDir);
                    
                    // Перевіряємо, чи ми в папці bin/Release або bin/Debug
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
                
                // Збереження файлу
                var fileName = $"Ticket_{ticket.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var filePath = Path.Combine(exportFolder, fileName);
                
                System.Diagnostics.Debug.WriteLine($"Спроба збереження текстового файлу: {filePath}");
                
                try
                {
                    File.WriteAllText(filePath, ticketText, System.Text.Encoding.UTF8);
                    
                    // Перевірка, чи файл дійсно створено
                    if (!File.Exists(filePath))
                    {
                        throw new IOException($"Файл не було створено: {filePath}");
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Текстовий файл успішно збережено: {filePath}, розмір: {new FileInfo(filePath).Length} байт");
                }
                catch (Exception saveEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Помилка збереження текстового файлу: {saveEx.GetType().Name}: {saveEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {saveEx.StackTrace}");
                    throw new IOException($"Не вдалося зберегти текстовий файл: {saveEx.Message}", saveEx);
                }
                
                return filePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"КРИТИЧНА ПОМИЛКА при генерації текстового файлу: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Внутрішня помилка: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                
                var errorMessage = $"Помилка генерації текстового файлу: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nДеталі: {ex.InnerException.Message}";
                }
                throw new InvalidOperationException(errorMessage, ex);
            }
        }
        
        private string GenerateTicketText(Ticket ticket)
        {
            var text = new System.Text.StringBuilder();
            
            // Заголовок
            text.AppendLine("═══════════════════════════════════════════════════════");
            text.AppendLine("              АВТОВОКЗАЛ");
            text.AppendLine("           ПОСАДОЧНИЙ ДОКУМЕНТ");
            text.AppendLine("═══════════════════════════════════════════════════════");
            text.AppendLine();
            
            // Маршрут
            text.AppendLine("МАРШРУТ:");
            string departure = ticket.Route.Departure ?? "Не вказано";
            string arrival = ticket.Route.Arrival ?? "Не вказано";
            text.AppendLine($"  {departure} → {arrival}");
            text.AppendLine();
            
            // Дата та час
            text.AppendLine("ДАТА ТА ЧАС ВІДПРАВЛЕННЯ:");
            string dateStr;
            if (ticket.Date == default(DateTime) || ticket.Date == DateTime.MinValue)
            {
                dateStr = DateTime.Now.ToString("dd.MM.yyyy");
            }
            else
            {
                dateStr = ticket.Date.ToString("dd.MM.yyyy");
            }
            text.AppendLine($"  Дата: {dateStr}");
            
            string timeStr;
            if (ticket.Route.DepartureTime == default(TimeSpan))
            {
                timeStr = "Не вказано";
            }
            else
            {
                timeStr = ticket.Route.DepartureTime.ToString(@"hh\:mm");
            }
            text.AppendLine($"  Час: {timeStr}");
            text.AppendLine();
            
            // Номер автобуса та місце
            int busNumber = 100 + (Math.Abs(ticket.Id) % 50);
            int seatNumber = 1 + (Math.Abs(ticket.Id) % 50);
            text.AppendLine("ТРАНСПОРТ:");
            text.AppendLine($"  Автобус: {busNumber}");
            text.AppendLine($"  Місце: {seatNumber}");
            text.AppendLine();
            
            text.AppendLine("───────────────────────────────────────────────────────");
            text.AppendLine();
            
            // Дані пасажира
            text.AppendLine("ДАНІ ПАСАЖИРА:");
            string lastName = ticket.Passenger.LastName ?? "";
            string firstName = ticket.Passenger.FirstName ?? "";
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
                    fullName += $" {ticket.Passenger.MiddleName}";
                }
            }
            
            text.AppendLine($"  Прізвище, Ім'я: {fullName}");
            
            if (!string.IsNullOrWhiteSpace(ticket.Passenger.Document))
            {
                text.AppendLine($"  Документ: {ticket.Passenger.Document.ToUpper()}");
            }
            text.AppendLine();
            
            text.AppendLine("───────────────────────────────────────────────────────");
            text.AppendLine();
            
            // UID квитка
            int ticketId = Math.Abs(ticket.Id);
            string uid = $"{ticketId:D4}-{DateTime.Now.Millisecond:D4}-{ticketId % 10000:D4}-{ticketId % 10000:D4}";
            text.AppendLine("UID КВИТКА:");
            text.AppendLine($"  {uid}");
            text.AppendLine();
            
            text.AppendLine("───────────────────────────────────────────────────────");
            text.AppendLine();
            
            // Фіскальна інформація
            text.AppendLine("ФІСКАЛЬНА ІНФОРМАЦІЯ:");
            text.AppendLine($"  Номер квитка: {ticket.Id}");
            
            string ticketType = (ticket.TicketType ?? "Regular") switch
            {
                "Discount" => "Пільговий",
                "Child" => "Дитячий",
                _ => "Звичайний"
            };
            text.AppendLine($"  Тип: {ticketType}");
            
            decimal price = ticket.Price;
            if (price < 0) price = 0;
            text.AppendLine($"  Вартість: {price:C}");
            text.AppendLine($"  Дата оформлення: {DateTime.Now:dd.MM.yyyy HH:mm}");
            
            string status = (ticket.Status ?? "Active") switch
            {
                "Active" => "Активний",
                "Cancelled" => "Скасований",
                "Used" => "Використаний",
                _ => ticket.Status ?? "Активний"
            };
            text.AppendLine($"  Статус: {status}");
            text.AppendLine();
            
            text.AppendLine("═══════════════════════════════════════════════════════");
            text.AppendLine();
            text.AppendLine("Дякуємо за вибір наших послуг!");
            text.AppendLine();
            
            return text.ToString();
        }
    }
}

