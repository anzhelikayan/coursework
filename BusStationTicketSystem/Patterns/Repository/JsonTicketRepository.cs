using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BusStationTicketSystem.Models;
using Newtonsoft.Json;

namespace BusStationTicketSystem.Patterns.Repository
{
    // Singleton Pattern - гарантує один екземпляр репозиторію та один JSON файл
    public class JsonTicketRepository : ITicketRepository
    {
        private const string DataFileName = "tickets_data.json";
        private TicketData _data = new TicketData();
        private int _nextTicketId = 1;
        private static readonly object _lock = new object();
        private static JsonTicketRepository? _instance;

        // Singleton Pattern - приватний конструктор
        private JsonTicketRepository()
        {
            LoadAll();
        }

        // Singleton Pattern - публічна властивість для отримання єдиного екземпляру
        public static JsonTicketRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new JsonTicketRepository();
                        }
                    }
                }
                return _instance;
            }
        }

        // Singleton Pattern - thread-safe операції з одним JSON файлом
        public void AddTicket(Ticket ticket)
        {
            lock (_lock)
            {
                if (ticket.Id == 0)
                {
                    ticket.Id = _nextTicketId++;
                }
                _data.Tickets.Add(ticket);
            }
        }

        public void RemoveTicket(int ticketId)
        {
            lock (_lock)
            {
                var ticket = _data.Tickets.FirstOrDefault(t => t.Id == ticketId);
                if (ticket != null)
                {
                    _data.Tickets.Remove(ticket);
                }
            }
        }

        public void UpdateTicket(Ticket ticket)
        {
            lock (_lock)
            {
                var existingTicket = _data.Tickets.FirstOrDefault(t => t.Id == ticket.Id);
                if (existingTicket != null)
                {
                    var index = _data.Tickets.IndexOf(existingTicket);
                    _data.Tickets[index] = ticket;
                }
            }
        }

        public Ticket? GetTicket(int ticketId)
        {
            lock (_lock)
            {
                // Перезавантажуємо дані з JSON файлу перед пошуком
                // щоб гарантувати, що ми працюємо з актуальними даними
                try
                {
                    if (File.Exists(DataFileName))
                    {
                        var json = File.ReadAllText(DataFileName);
                        
                        var settings = new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Populate,
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            Error = (sender, args) =>
                            {
                                System.Diagnostics.Debug.WriteLine($"Помилка десеріалізації при GetTicket: {args.ErrorContext.Error.Message}");
                                args.ErrorContext.Handled = true;
                            },
                            Converters = new JsonConverter[] 
                            { 
                                new DateTimeConverter(),
                                new TimeSpanConverter() 
                            }
                        };
                        
                        var deserializedData = JsonConvert.DeserializeObject<TicketData>(json, settings);
                        if (deserializedData != null)
                        {
                            // Оновлюємо дані в пам'яті
                            _data = deserializedData;
                            
                            // Перевірка на null
                            if (_data.Routes == null)
                                _data.Routes = new List<Route>();
                            if (_data.Tickets == null)
                                _data.Tickets = new List<Ticket>();
                            
                            System.Diagnostics.Debug.WriteLine($"Оновлено дані з JSON: {_data.Tickets.Count} квитків");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Помилка перезавантаження даних при GetTicket: {ex.Message}");
                    // Продовжуємо з даними в пам'яті
                }
                
                // Шукаємо квиток
                var ticket = _data.Tickets?.FirstOrDefault(t => t.Id == ticketId);
                
                if (ticket != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Знайдено квиток ID={ticketId}: Route={ticket.Route?.DisplayName ?? "null"}, Passenger={ticket.Passenger?.FullName ?? "null"}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Квиток з ID={ticketId} не знайдено. Всього квитків: {_data.Tickets?.Count ?? 0}");
                }
                
                return ticket;
            }
        }

        // Singleton Pattern - thread-safe читання з одного JSON файлу
        public List<Ticket> GetAllTickets()
        {
            lock (_lock)
            {
                if (_data == null || _data.Tickets == null)
                {
                    System.Diagnostics.Debug.WriteLine("Дані або список квитків null");
                    return new List<Ticket>();
                }
                
                var tickets = _data.Tickets.ToList();
                System.Diagnostics.Debug.WriteLine($"Повертаю {tickets.Count} квитків з репозиторію");
                return tickets;
            }
        }

        public List<Route> GetAllRoutes()
        {
            lock (_lock)
            {
                return _data.Routes.ToList();
            }
        }

        // Singleton Pattern - thread-safe збереження в один JSON файл
        public void SaveAll()
        {
            lock (_lock)
            {
                try
                {
                    // Перевірка на null перед серіалізацією
                    if (_data == null)
                    {
                        _data = new TicketData();
                    }
                    if (_data.Routes == null)
                        _data.Routes = new List<Route>();
                    if (_data.Tickets == null)
                        _data.Tickets = new List<Ticket>();

                    // Налаштування для серіалізації з правильними форматами
                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        Converters = new JsonConverter[] 
                        { 
                            new DateTimeConverter(),
                            new TimeSpanConverter() 
                        }
                    };
                    
                    var json = JsonConvert.SerializeObject(_data, settings);
                    
                    // Використовуємо FileStream для більш надійного запису
                    // Singleton гарантує, що всі операції зберігаються в один файл
                    using (var fileStream = new FileStream(DataFileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                    using (var writer = new StreamWriter(fileStream))
                    {
                        writer.Write(json);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Немає доступу до файлу: {ex.Message}");
                    // Не викидаємо виняток, щоб не блокувати роботу додатка
                }
                catch (IOException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Помилка вводу/виводу: {ex.Message}");
                    // Не викидаємо виняток, щоб не блокувати роботу додатка
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Помилка збереження даних: {ex.Message}");
                    // Не викидаємо виняток, щоб не блокувати роботу додатка
                }
            }
        }

        public void LoadAll()
        {
            try
            {
                if (File.Exists(DataFileName))
                {
                    var json = File.ReadAllText(DataFileName);
                    
                    // Налаштування для десеріалізації з правильними форматами
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Populate,
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        Error = (sender, args) =>
                        {
                            // Обробка помилок десеріалізації
                            var errorMsg = $"Помилка десеріалізації: {args.ErrorContext.Error.Message}";
                            var pathMsg = $"Path: {args.ErrorContext.Path}";
                            var memberMsg = $"Member: {args.ErrorContext.Member}";
                            
                            System.Diagnostics.Debug.WriteLine("=== ПОМИЛКА ДЕСЕРІАЛІЗАЦІЇ ===");
                            System.Diagnostics.Debug.WriteLine(errorMsg);
                            System.Diagnostics.Debug.WriteLine(pathMsg);
                            System.Diagnostics.Debug.WriteLine(memberMsg);
                            System.Diagnostics.Debug.WriteLine($"OriginalObject: {args.ErrorContext.OriginalObject?.GetType().Name ?? "null"}");
                            System.Diagnostics.Debug.WriteLine($"Error: {args.ErrorContext.Error.GetType().Name}");
                            
                            // Для помилок формату продовжуємо з дефолтними значеннями
                            if (args.ErrorContext.Error is FormatException formatEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Помилка формату, встановлюю дефолтне значення: {formatEx.Message}");
                                args.ErrorContext.Handled = true;
                            }
                            else if (args.ErrorContext.Error is JsonReaderException)
                            {
                                System.Diagnostics.Debug.WriteLine("Помилка читання JSON, продовжую...");
                                args.ErrorContext.Handled = true;
                            }
                            else if (args.ErrorContext.Error is JsonSerializationException)
                            {
                                // Для помилок серіалізації також продовжуємо
                                System.Diagnostics.Debug.WriteLine($"Помилка серіалізації: {args.ErrorContext.Error.Message}");
                                args.ErrorContext.Handled = true;
                            }
                            else
                            {
                                // Для інших помилок також продовжуємо, але логуємо
                                System.Diagnostics.Debug.WriteLine($"Інша помилка, продовжую з обробкою: {args.ErrorContext.Error.GetType().Name}");
                                args.ErrorContext.Handled = true;
                            }
                        },
                        Converters = new JsonConverter[] 
                        { 
                            new DateTimeConverter(),
                            new TimeSpanConverter() 
                        }
                    };
                    
                    System.Diagnostics.Debug.WriteLine("Початок десеріалізації JSON...");
                    System.Diagnostics.Debug.WriteLine($"Довжина JSON: {json.Length} символів");
                    
                    try
                    {
                        var deserializedData = JsonConvert.DeserializeObject<TicketData>(json, settings);
                        _data = deserializedData ?? new TicketData();
                        System.Diagnostics.Debug.WriteLine("Десеріалізація завершена успішно");
                    }
                    catch (Exception deserializeEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"КРИТИЧНА ПОМИЛКА десеріалізації: {deserializeEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"Тип помилки: {deserializeEx.GetType().Name}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {deserializeEx.StackTrace}");
                        if (deserializeEx.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Внутрішня помилка: {deserializeEx.InnerException.Message}");
                        }
                        _data = new TicketData();
                    }
                    
                    if (_data == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Десеріалізація повернула null, створюю новий об'єкт");
                        _data = new TicketData();
                    }
                    
                    // Перевірка на null
                    if (_data.Routes == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Routes null, створюю новий список");
                        _data.Routes = new List<Route>();
                    }
                    if (_data.Tickets == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Tickets null, створюю новий список");
                        _data.Tickets = new List<Ticket>();
                    }
                    
                    // Діагностика завантаження
                    System.Diagnostics.Debug.WriteLine($"Завантажено {_data.Routes.Count} маршрутів");
                    System.Diagnostics.Debug.WriteLine($"Завантажено {_data.Tickets.Count} квитків");
                    
                    // Детальна діагностика квитків
                    if (_data.Tickets.Count > 0)
                    {
                        for (int i = 0; i < Math.Min(3, _data.Tickets.Count); i++)
                        {
                            var ticket = _data.Tickets[i];
                            System.Diagnostics.Debug.WriteLine($"Квиток {i + 1}: ID={ticket.Id}, Route={ticket.Route?.DisplayName ?? "null"}, Passenger={ticket.Passenger?.FullName ?? "null"}, Date={ticket.Date:yyyy-MM-dd}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("УВАГА: Список квитків порожній після десеріалізації!");
                    }
                    
                    if (_data.Tickets.Any())
                    {
                        _nextTicketId = _data.Tickets.Max(t => t.Id) + 1;
                        System.Diagnostics.Debug.WriteLine($"Наступний ID квитка: {_nextTicketId}");
                    }
                    else
                    {
                        _nextTicketId = 1;
                        System.Diagnostics.Debug.WriteLine("Квитки не знайдено, наступний ID: 1");
                    }
                }
                else
                {
                    InitializeDefaultRoutes();
                }
            }
            catch (JsonException ex)
            {
                // Якщо JSON файл пошкоджений, спробуємо зберегти те, що вдалося завантажити
                if (_data == null)
                {
                    _data = new TicketData();
                }
                if (_data.Routes == null)
                {
                    _data.Routes = new List<Route>();
                }
                if (_data.Tickets == null)
                {
                    _data.Tickets = new List<Ticket>();
                }
                // Якщо немає маршрутів, ініціалізуємо дефолтні
                if (_data.Routes.Count == 0)
                {
                    InitializeDefaultRoutes();
                }
                System.Diagnostics.Debug.WriteLine($"Помилка парсингу JSON: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Завантажено: {_data.Routes.Count} маршрутів, {_data.Tickets.Count} квитків");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            catch (FormatException ex)
            {
                // Помилка формату (наприклад, TimeSpan або DateTime) - зберігаємо те, що вдалося завантажити
                if (_data == null)
                {
                    _data = new TicketData();
                }
                if (_data.Routes == null)
                {
                    _data.Routes = new List<Route>();
                }
                if (_data.Tickets == null)
                {
                    _data.Tickets = new List<Ticket>();
                }
                // Якщо немає маршрутів, ініціалізуємо дефолтні
                if (_data.Routes.Count == 0)
                {
                    InitializeDefaultRoutes();
                }
                System.Diagnostics.Debug.WriteLine($"Помилка формату даних: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Завантажено: {_data.Routes.Count} маршрутів, {_data.Tickets.Count} квитків");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                // У разі інших помилок спробуємо зберегти те, що вдалося завантажити
                if (_data == null)
                {
                    _data = new TicketData();
                }
                if (_data.Routes == null)
                {
                    _data.Routes = new List<Route>();
                }
                if (_data.Tickets == null)
                {
                    _data.Tickets = new List<Ticket>();
                }
                // Якщо немає маршрутів, ініціалізуємо дефолтні
                if (_data.Routes.Count == 0)
                {
                    InitializeDefaultRoutes();
                }
                System.Diagnostics.Debug.WriteLine($"Помилка завантаження даних: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Завантажено: {_data.Routes.Count} маршрутів, {_data.Tickets.Count} квитків");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void InitializeDefaultRoutes()
        {
            try
            {
                _data.Routes = new List<Route>
                {
                    new Route { Id = 1, Departure = "Київ", Arrival = "Львів", Distance = 540, BasePrice = 450, DepartureTime = new TimeSpan(8, 0, 0) },
                    new Route { Id = 2, Departure = "Київ", Arrival = "Одеса", Distance = 480, BasePrice = 380, DepartureTime = new TimeSpan(10, 30, 0) },
                    new Route { Id = 3, Departure = "Львів", Arrival = "Київ", Distance = 540, BasePrice = 450, DepartureTime = new TimeSpan(14, 0, 0) },
                    new Route { Id = 4, Departure = "Одеса", Arrival = "Київ", Distance = 480, BasePrice = 380, DepartureTime = new TimeSpan(16, 0, 0) },
                    new Route { Id = 5, Departure = "Харків", Arrival = "Київ", Distance = 480, BasePrice = 400, DepartureTime = new TimeSpan(9, 0, 0) }
                };
                _data.Tickets = new List<Ticket>();
                SaveAll();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка ініціалізації маршрутів: {ex.Message}");
                // Продовжуємо з порожніми списками
                _data.Routes = new List<Route>();
                _data.Tickets = new List<Ticket>();
            }
        }
    }
}


