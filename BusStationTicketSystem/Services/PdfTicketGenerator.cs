using System;
using System.IO;
using BusStationTicketSystem.Models;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Fonts;

namespace BusStationTicketSystem.Services
{
    public class PdfTicketGenerator
    {
        private static bool _fontResolverInitialized = false;
        private static readonly object _fontResolverLock = new object();

        private static void EnsureFontResolver()
        {
            // Лінива ініціалізація FontResolver при першому використанні
            if (!_fontResolverInitialized)
            {
                lock (_fontResolverLock)
                {
                    if (!_fontResolverInitialized)
                    {
                        try
                        {
                            // Спробуємо встановити FontResolver тільки якщо він ще не встановлений
                            if (GlobalFontSettings.FontResolver == null)
                            {
                                GlobalFontSettings.FontResolver = new SystemFontResolver();
                            }
                        }
                        catch (Exception ex)
                        {
                            // Якщо не вдалося встановити FontResolver, продовжуємо без нього
                            // Вбудовані шрифти (Helvetica) працюватимуть і без FontResolver
                            System.Diagnostics.Debug.WriteLine($"Не вдалося ініціалізувати FontResolver: {ex.Message}");
                        }
                        finally
                        {
                            _fontResolverInitialized = true; // Встановлюємо прапорець в будь-якому випадку
                        }
                    }
                }
            }
        }

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
            
            PdfDocument? document = null;
            XGraphics? gfx = null;
            
            try
            {
                // Ініціалізуємо FontResolver перед використанням
                EnsureFontResolver();
                
                // Створення нового PDF документа - вертикальний формат як справжній квиток
                document = new PdfDocument();
                if (document == null)
                {
                    throw new InvalidOperationException("Не вдалося створити PDF документ");
                }
                
                var page = document.AddPage();
                if (page == null)
                {
                    throw new InvalidOperationException("Не вдалося додати сторінку до PDF документа");
                }
                
                page.Size = PdfSharp.PageSize.A4;
                
                gfx = XGraphics.FromPdfPage(page);
                if (gfx == null)
                {
                    throw new InvalidOperationException("Не вдалося створити графічний контекст для PDF");
                }
                
                // Використовуємо вбудовані шрифти PdfSharp
                XFont fontCompany, fontTitle, fontHeader, fontNormal, fontSmall, fontBold;
                try
                {
                    fontCompany = new XFont("Helvetica", 18, XFontStyleEx.Bold);
                    fontTitle = new XFont("Helvetica", 16, XFontStyleEx.Bold);
                    fontHeader = new XFont("Helvetica", 12, XFontStyleEx.Bold);
                    fontNormal = new XFont("Helvetica", 11, XFontStyleEx.Regular);
                    fontSmall = new XFont("Helvetica", 9, XFontStyleEx.Regular);
                    fontBold = new XFont("Helvetica", 11, XFontStyleEx.Bold);
                }
                catch (Exception fontEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Помилка створення шрифтів Helvetica: {fontEx.Message}, використовую Times-Roman");
                    fontCompany = new XFont("Times-Roman", 18, XFontStyleEx.Bold);
                    fontTitle = new XFont("Times-Roman", 16, XFontStyleEx.Bold);
                    fontHeader = new XFont("Times-Roman", 12, XFontStyleEx.Bold);
                    fontNormal = new XFont("Times-Roman", 11, XFontStyleEx.Regular);
                    fontSmall = new XFont("Times-Roman", 9, XFontStyleEx.Regular);
                    fontBold = new XFont("Times-Roman", 11, XFontStyleEx.Bold);
                }
                
                // Кольори
                var colorBlack = XBrushes.Black;
                var colorGray = XBrushes.Gray;
                var colorDarkGray = XBrushes.DarkGray;
                var penBlack = new XPen(XColors.Black, 1);
                var penGray = new XPen(XColors.Gray, 0.5);
                
                // Перевірка розміру сторінки
                if (page.Width <= 0 || page.Height <= 0)
                {
                    throw new InvalidOperationException($"Некоректний розмір сторінки: {page.Width}x{page.Height}");
                }
                
                double yPos = 30;
                double leftMargin = 40;
                double rightMargin = page.Width - 40;
                double contentWidth = rightMargin - leftMargin;
                
                if (contentWidth <= 0)
                {
                    throw new InvalidOperationException($"Некоректна ширина контенту: {contentWidth}");
                }
                
                // Заголовок - назва компанії
                gfx.DrawString("АВТОВОКЗАЛ", fontCompany, colorBlack, 
                    new XRect(leftMargin, yPos, contentWidth, 25), 
                    XStringFormats.TopCenter);
                yPos += 25;
                
                // Підзаголовок
                gfx.DrawString("ПОСАДОЧНИЙ ДОКУМЕНТ", fontTitle, colorBlack, 
                    new XRect(leftMargin, yPos, contentWidth, 20), 
                    XStringFormats.TopCenter);
                yPos += 30;
                
                // Розділювальна лінія
                gfx.DrawLine(penBlack, leftMargin, yPos, rightMargin, yPos);
                yPos += 20;
                
                // Основний блок інформації
                double leftColumn = leftMargin;
                double rightColumn = leftMargin + contentWidth / 2;
                
                // Маршрут (Route гарантовано не null після перевірки на початку)
                gfx.DrawString("Маршрут:", fontBold, colorBlack, leftColumn, yPos);
                yPos += 18;
                string departure = ticket.Route.Departure ?? "Не вказано";
                string arrival = ticket.Route.Arrival ?? "Не вказано";
                string routeInfo = $"{departure} — {arrival}";
                gfx.DrawString(routeInfo, fontNormal, colorBlack, leftColumn, yPos);
                yPos += 25;
                
                // Дата та час
                gfx.DrawString("Дата відправлення:", fontBold, colorBlack, leftColumn, yPos);
                yPos += 18;
                string dateStr;
                if (ticket.Date == default(DateTime) || ticket.Date == DateTime.MinValue)
                {
                    dateStr = DateTime.Now.ToString("dd.MM.yyyy");
                }
                else
                {
                    dateStr = ticket.Date.ToString("dd.MM.yyyy");
                }
                gfx.DrawString(dateStr, fontNormal, colorBlack, leftColumn, yPos);
                yPos += 20;
                
                // Час (Route гарантовано не null після перевірки на початку)
                try
                {
                    string timeStr;
                    if (ticket.Route.DepartureTime == default(TimeSpan))
                    {
                        timeStr = "Не вказано";
                    }
                    else
                    {
                        timeStr = ticket.Route.DepartureTime.ToString(@"hh\:mm");
                    }
                    gfx.DrawString("Час:", fontBold, colorBlack, leftColumn, yPos);
                    yPos += 18;
                    gfx.DrawString(timeStr, fontNormal, colorBlack, leftColumn, yPos);
                    yPos += 25;
                }
                catch (Exception timeEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Помилка форматування часу: {timeEx.Message}");
                    gfx.DrawString("Час:", fontBold, colorBlack, leftColumn, yPos);
                    yPos += 18;
                    gfx.DrawString("Не вказано", fontNormal, colorBlack, leftColumn, yPos);
                    yPos += 25;
                }
                
                // Номер автобуса (генеруємо на основі ID)
                int busNumber = 100 + (Math.Abs(ticket.Id) % 50);
                gfx.DrawString("Автобус:", fontBold, colorBlack, leftColumn, yPos);
                yPos += 18;
                gfx.DrawString($"{busNumber}", fontNormal, colorBlack, leftColumn, yPos);
                yPos += 20;
                
                // Місце (генеруємо на основі ID)
                int seatNumber = 1 + (Math.Abs(ticket.Id) % 50);
                gfx.DrawString("Місце:", fontBold, colorBlack, leftColumn, yPos);
                yPos += 18;
                gfx.DrawString($"{seatNumber}", fontNormal, colorBlack, leftColumn, yPos);
                yPos += 30;
                
                // Розділювальна лінія
                gfx.DrawLine(penGray, leftMargin, yPos, rightMargin, yPos);
                yPos += 20;
                
                // Дані пасажира (Passenger гарантовано не null після перевірки на початку)
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
                
                gfx.DrawString("Прізвище, Ім'я:", fontBold, colorBlack, leftColumn, yPos);
                yPos += 18;
                gfx.DrawString(fullName, fontNormal, colorBlack, leftColumn, yPos);
                yPos += 25;
                
                if (!string.IsNullOrWhiteSpace(ticket.Passenger.Document))
                {
                    gfx.DrawString("Документ:", fontBold, colorBlack, leftColumn, yPos);
                    yPos += 18;
                    gfx.DrawString(ticket.Passenger.Document.ToUpper(), fontNormal, colorBlack, leftColumn, yPos);
                    yPos += 30;
                }
                
                // Розділювальна лінія
                gfx.DrawLine(penGray, leftMargin, yPos, rightMargin, yPos);
                yPos += 20;
                
                // QR-код (симуляція - малюємо квадрат з текстом)
                double qrSize = 80;
                double qrX = rightMargin - qrSize - 10;
                double qrY = yPos;
                
                // Рамка QR-коду
                gfx.DrawRectangle(penBlack, qrX, qrY, qrSize, qrSize);
                
                // Симуляція QR-коду (малюємо квадратики)
                int ticketIdForRandom = Math.Abs(ticket.Id);
                if (ticketIdForRandom == 0) ticketIdForRandom = 1; // Уникаємо 0 для Random
                var random = new Random(ticketIdForRandom);
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (random.Next(2) == 1)
                        {
                            gfx.DrawRectangle(XBrushes.Black, 
                                qrX + 5 + i * 8.75, 
                                qrY + 5 + j * 8.75, 
                                7, 7);
                        }
                    }
                }
                
                yPos += qrSize + 20;
                
                // UID квитка
                int ticketId = Math.Abs(ticket.Id);
                string uid = $"{ticketId:D4}-{DateTime.Now.Millisecond:D4}-{ticketId % 10000:D4}-{ticketId % 10000:D4}";
                gfx.DrawString("UID квитка", fontSmall, colorDarkGray, leftColumn, yPos);
                yPos += 15;
                gfx.DrawString(uid, fontSmall, colorBlack, leftColumn, yPos);
                yPos += 25;
                
                // Фіскальна інформація
                gfx.DrawLine(penGray, leftMargin, yPos, rightMargin, yPos);
                yPos += 15;
                
                gfx.DrawString($"Номер квитка: {ticket.Id}", fontSmall, colorDarkGray, leftColumn, yPos);
                yPos += 15;
                
                string ticketType = (ticket.TicketType ?? "Regular") switch
                {
                    "Discount" => "Пільговий",
                    "Child" => "Дитячий",
                    _ => "Звичайний"
                };
                gfx.DrawString($"Тип: {ticketType}", fontSmall, colorDarkGray, leftColumn, yPos);
                yPos += 15;
                
                decimal price = ticket.Price;
                if (price < 0) price = 0; // Запобігаємо негативним цінам
                gfx.DrawString($"Вартість: {price:C}", fontBold, colorBlack, leftColumn, yPos);
                yPos += 20;
                
                gfx.DrawString($"Дата оформлення: {DateTime.Now:dd.MM.yyyy HH:mm}", fontSmall, colorDarkGray, leftColumn, yPos);
                yPos += 15;
                
                string status = (ticket.Status ?? "Active") switch
                {
                    "Active" => "Активний",
                    "Cancelled" => "Скасований",
                    "Used" => "Використаний",
                    _ => ticket.Status ?? "Активний"
                };
                gfx.DrawString($"Статус: {status}", fontSmall, colorDarkGray, leftColumn, yPos);
                
                // Створення папки для експортованих квитків в папці проекту
                string projectFolder;
                try
                {
                    // Отримуємо шлях до виконуваного файлу
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var baseDirInfo = new DirectoryInfo(baseDir);
                    
                    // Перевіряємо, чи ми в папці bin/Release або bin/Debug
                    if (baseDirInfo.Name == "net8.0-windows" && 
                        (baseDirInfo.Parent?.Name == "Release" || baseDirInfo.Parent?.Name == "Debug"))
                    {
                        // Ми в bin/Release/net8.0-windows або bin/Debug/net8.0-windows
                        // Переходимо до папки проекту (на 2 рівні вище)
                        projectFolder = baseDirInfo.Parent?.Parent?.FullName ?? baseDir;
                    }
                    else
                    {
                        // Намагаємося знайти папку проекту, шукаючи файл .csproj
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
                    // Якщо не вдалося створити папку, використовуємо поточну директорію
                    exportFolder = Path.Combine(projectFolder, "ExportedTickets");
                }
                
                // Збереження файлу
                var fileName = $"Ticket_{ticket.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(exportFolder, fileName);
                
                System.Diagnostics.Debug.WriteLine($"Спроба збереження PDF: {filePath}");
                
                try
                {
                    // Зберігаємо документ
                    if (document == null)
                    {
                        throw new InvalidOperationException("PDF документ не ініціалізовано");
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Збереження PDF в: {filePath}");
                    document.Save(filePath);
                    
                    // Перевірка, чи файл дійсно створено
                    if (!File.Exists(filePath))
                    {
                        throw new IOException($"Файл не було створено: {filePath}");
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"PDF успішно збережено: {filePath}, розмір: {new FileInfo(filePath).Length} байт");
                }
                catch (Exception saveEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Помилка збереження PDF: {saveEx.GetType().Name}: {saveEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {saveEx.StackTrace}");
                    throw new IOException($"Не вдалося зберегти PDF файл: {saveEx.Message}", saveEx);
                }
                finally
                {
                    // Закриваємо ресурси
                    try
                    {
                        gfx?.Dispose();
                    }
                    catch (Exception gfxEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Помилка закриття gfx: {gfxEx.Message}");
                    }
                    
                    try
                    {
                        document?.Dispose();
                    }
                    catch (Exception docEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Помилка закриття document: {docEx.Message}");
                    }
                }
                
                return filePath;
            }
            catch (Exception ex)
            {
                // Гарантуємо звільнення ресурсів навіть при помилці
                try
                {
                    gfx?.Dispose();
                }
                catch { }
                
                try
                {
                    document?.Dispose();
                }
                catch { }
                
                System.Diagnostics.Debug.WriteLine($"КРИТИЧНА ПОМИЛКА при генерації PDF: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Внутрішня помилка: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                
                // Створюємо більш інформативну помилку
                var errorMessage = $"Помилка генерації PDF: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nДеталі: {ex.InnerException.Message}";
                }
                throw new InvalidOperationException(errorMessage, ex);
            }
        }
    }
}

