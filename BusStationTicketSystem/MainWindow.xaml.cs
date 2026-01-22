using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using BusStationTicketSystem.Models;
using BusStationTicketSystem.Patterns.Builder;
using BusStationTicketSystem.Patterns.Command;
using BusStationTicketSystem.Patterns.Factory;
using BusStationTicketSystem.Patterns.Observer;
using BusStationTicketSystem.Patterns.Repository;
using BusStationTicketSystem.Patterns.Strategy;
using BusStationTicketSystem.Patterns.TemplateMethod;
using BusStationTicketSystem.Patterns.Decorator;
using BusStationTicketSystem.Patterns.State;
using BusStationTicketSystem.Patterns.Visitor;
using BusStationTicketSystem.Patterns.Composite;
using BusStationTicketSystem.ViewModels;
using BusStationTicketSystem.Services;

namespace BusStationTicketSystem
{
    public partial class MainWindow : Window
    {
        private readonly ITicketRepository _repository = null!;
        private readonly TicketSubject _ticketSubject = null!;
        private readonly TicketBuilder _ticketBuilder = null!;
        private ICommand? _lastCommand;
        private readonly ObservableCollection<PassengerTicketItem> _passengerTickets = new ObservableCollection<PassengerTicketItem>();
        private List<Ticket> _allTickets = new List<Ticket>(); // Зберігаємо всі квитки для фільтрації
        
        // Список можливих статусів для ComboBox
        public List<string> StatusList { get; } = new List<string> { "Active", "Cancelled", "Used" };

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                
                // Repository Pattern + Singleton Pattern - ініціалізація репозиторію
                // Singleton гарантує один екземпляр та один JSON файл для всіх операцій
                _repository = JsonTicketRepository.Instance;
                
                // Observer Pattern - налаштування спостерігачів
                _ticketSubject = new TicketSubject();
                _ticketSubject.Attach(new LoggingObserver());
                _ticketSubject.Attach(new JsonSaveObserver(_repository));
                
                // Builder Pattern - ініціалізація будівника
                _ticketBuilder = new TicketBuilder();
                
                // Composite + Builder Pattern - створення меню
                BuildApplicationMenu();
                
                LoadData();
                
                // Запуск анімації заголовка
                StartTitleAnimation();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка ініціалізації: {ex.Message}\n\nДеталі: {ex.StackTrace}", 
                    "Критична помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void StartTitleAnimation()
        {
            // Без анімації для стандартного професійного вигляду
            // Заголовок залишається статичним
        }

        private void InitializeStatusComboBox()
        {
            // Ініціалізація ComboBox для статусів в DataGrid
            if (TicketsDataGrid != null && TicketsDataGrid.Columns.Count > 0)
            {
                foreach (var column in TicketsDataGrid.Columns)
                {
                    if (column is System.Windows.Controls.DataGridComboBoxColumn comboColumn && 
                        comboColumn.Header?.ToString() == "Статус")
                    {
                        // Встановлюємо список статусів для ComboBox
                        comboColumn.ItemsSource = StatusList;
                        break;
                    }
                }
            }
        }

        private void BuildApplicationMenu()
        {
            try
            {
                // Composite + Builder Pattern - побудова меню додатка
                var menuBuilder = new MenuBuilder();
                
                menuBuilder
                    .CreateMenu("Головне")
                    .AddSubMenu("Файл")
                        .AddMenuItem("Новий квиток", () => {
                            try
                            {
                                // Перехід на вкладку продажу
                                MainTabControl.SelectedIndex = 0;
                                // Очищення полів
                                PassengerLastNameTextBox.Clear();
                                PassengerFirstNameTextBox.Clear();
                                PassengerMiddleNameTextBox.Clear();
                                PassengerPhoneTextBox.Clear();
                                PassengerDocumentTextBox.Clear();
                                RoutesComboBox.SelectedItem = null;
                                DatePicker.SelectedDate = DateTime.Now;
                                QuantityTextBox.Text = "1";
                                InsuranceCheckBox.IsChecked = false;
                                BaggageCheckBox.IsChecked = false;
                                TotalPriceTextBlock.Text = "";
                                StatusTextBlock.Text = "Створено новий квиток";
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Помилка в меню 'Новий квиток': {ex.Message}");
                            }
                        })
                        .AddSeparator()
                        .AddMenuItem("Експорт даних", ExportData)
                        .AddSeparator()
                        .AddMenuItem("Вихід", () => Application.Current.Shutdown())
                    .EndSubMenu()
                    .AddSubMenu("Квитки")
                        .AddMenuItem("Список квитків", () => {
                            try
                            {
                                RefreshTickets();
                                StatusTextBlock.Text = "Оновлено список квитків";
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Помилка оновлення списку: {ex.Message}");
                            }
                        })
                        .AddMenuItem("Скасувати квиток", () => {
                            try
                            {
                                if (TicketsDataGrid.SelectedItem is Ticket ticket)
                                {
                                    CancelTicketButton_Click(null!, null!);
                                }
                                else
                                {
                                    MessageBox.Show("Виберіть квиток для скасування!", "Помилка", 
                                        MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Помилка скасування квитка: {ex.Message}");
                            }
                        })
                        .AddSeparator()
                        .AddMenuItem("Статистика", ShowStatistics)
                    .EndSubMenu()
                    .AddSubMenu("Маршрути")
                        .AddMenuItem("Перегляд маршрутів", () => {
                            try
                            {
                                var routes = _repository.GetAllRoutes();
                                RoutesDataGrid.ItemsSource = routes;
                                StatusTextBlock.Text = $"Завантажено {routes.Count} маршрутів";
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Помилка завантаження маршрутів: {ex.Message}");
                            }
                        })
                        .AddMenuItem("Оновити маршрути", () => {
                            try
                            {
                                LoadData();
                                StatusTextBlock.Text = "Маршрути оновлено";
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Помилка оновлення маршрутів: {ex.Message}");
                            }
                        })
                    .EndSubMenu()
                    .AddSubMenu("Допомога")
                        .AddMenuItem("Про програму", ShowAbout)
                        .AddMenuItem("Інструкція", ShowInstructions);

                // Рендеринг меню в MenuBar
                menuBuilder.RenderToMenuBar(MainMenuBar);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка створення меню: {ex.Message}");
                // Продовжуємо роботу навіть якщо меню не вдалося створити
            }
        }

        private void ExportData()
        {
            try
            {
                // Visitor Pattern - використання для експорту
                var visitor = new JsonSerializationVisitor();
                var tickets = _repository.GetAllTickets();
                var routes = _repository.GetAllRoutes();

                foreach (var route in routes)
                {
                    visitor.Visit(route);
                }

                foreach (var ticket in tickets)
                {
                    visitor.Visit(ticket);
                    if (ticket.Route != null)
                        visitor.Visit(ticket.Route);
                    if (ticket.Passenger != null)
                        visitor.Visit(ticket.Passenger);
                }

                var json = visitor.GetJson();
                var fileName = $"export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                System.IO.File.WriteAllText(fileName, json);
                
                MessageBox.Show($"Дані експортовано в файл: {fileName}", "Експорт", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                StatusTextBlock.Text = $"Експортовано {tickets.Count} квитків";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка експорту: {ex.Message}", "Помилка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowStatistics()
        {
            var tickets = _repository.GetAllTickets();
            var activeTickets = tickets.Count(t => t.Status == "Active");
            var cancelledTickets = tickets.Count(t => t.Status == "Cancelled");
            var totalRevenue = tickets.Where(t => t.Status == "Active").Sum(t => t.Price);

            var stats = $"Статистика квитків:\n\n" +
                       $"Всього квитків: {tickets.Count}\n" +
                       $"Активних: {activeTickets}\n" +
                       $"Скасованих: {cancelledTickets}\n" +
                       $"Загальний дохід: {totalRevenue:C}";

            MessageBox.Show(stats, "Статистика", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowAbout()
        {
            var about = "Система продажу квитків автовокзалу\n\n" +
                       "Версія: 1.0\n" +
                       "Розроблено з використанням 10 шаблонів проєктування:\n" +
                       "Observer, Visitor, Strategy, Factory, Builder,\n" +
                       "Command, Repository, Decorator, Template Method, State,\n" +
                       "Composite (для меню)";

            MessageBox.Show(about, "Про програму", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowInstructions()
        {
            var instructions = "Інструкція користувача:\n\n" +
                             "1. Виберіть маршрут, дату та тип квитка\n" +
                             "2. Введіть дані пасажира\n" +
                             "3. Оберіть спосіб оплати\n" +
                             "4. Додайте додаткові послуги (за потреби)\n" +
                             "5. Натисніть 'Купити квиток'\n\n" +
                             "Всі дані автоматично зберігаються в JSON файл.";

            MessageBox.Show(instructions, "Інструкція", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadData()
        {
            try
            {
                // Перевірка на null перед використанням
                if (_repository == null)
                {
                    System.Diagnostics.Debug.WriteLine("Репозиторій не ініціалізовано");
                    return;
                }

                // Завантаження маршрутів
                var routes = _repository.GetAllRoutes();
                if (routes != null)
                {
                    RoutesComboBox.ItemsSource = routes;
                    RoutesDataGrid.ItemsSource = routes;
                }
                
                // Завантаження квитків
                RefreshTickets();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка завантаження даних: {ex.Message}");
                // Не показуємо MessageBox під час ініціалізації, щоб не блокувати завантаження
            }
        }

        private void RoutesComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateTotalPrice();
        }

        private void QuantityTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Перевірка на ініціалізацію компонентів (може викликатися під час InitializeComponent)
            if (SinglePassengerPanel == null || MultiplePassengersItemsControl == null || QuantityTextBox == null)
                return;

            if (int.TryParse(QuantityTextBox.Text, out int quantity))
            {
                if (quantity > 10)
                {
                    MessageBox.Show("Максимальна кількість квитків - 10", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    QuantityTextBox.Text = "10";
                    return;
                }
                else if (quantity > 1)
                {
                    // Показати форми для кількох пасажирів
                    SinglePassengerPanel.Visibility = Visibility.Collapsed;
                    MultiplePassengersItemsControl.Visibility = Visibility.Visible;

                    // Оновити список пасажирів
                    UpdatePassengersList(quantity);
                }
                else
                {
                    // Показати форму для одного пасажира
                    SinglePassengerPanel.Visibility = Visibility.Visible;
                    MultiplePassengersItemsControl.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                // Якщо не вдалося розпарсити, показуємо форму для одного пасажира
                SinglePassengerPanel.Visibility = Visibility.Visible;
                MultiplePassengersItemsControl.Visibility = Visibility.Collapsed;
            }

            // Оновлення ціни тільки якщо компоненти ініціалізовані
            if (RoutesComboBox != null && DatePicker != null)
            {
                UpdateTotalPrice();
            }
        }

        private void UpdatePassengersList(int quantity)
        {
            // Перевірка на ініціалізацію
            if (MultiplePassengersItemsControl == null)
                return;

            _passengerTickets.Clear();
            for (int i = 1; i <= quantity; i++)
            {
                _passengerTickets.Add(new PassengerTicketItem 
                { 
                    TicketNumber = i, 
                    SelectedTicketTypeIndex = 0 
                });
            }
            MultiplePassengersItemsControl.ItemsSource = _passengerTickets;
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox comboBox && comboBox.Tag is PassengerTicketItem item)
            {
                comboBox.SelectedIndex = item.SelectedTicketTypeIndex;
            }
        }

        private void IndividualTicketType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox comboBox && comboBox.Tag is PassengerTicketItem item)
            {
                item.SelectedTicketTypeIndex = comboBox.SelectedIndex;
            }
            UpdateTotalPrice();
        }

        private void UpdateDecoratorPrice(object sender, RoutedEventArgs e)
        {
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice(object? sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice(object? sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            if (RoutesComboBox.SelectedItem is Route selectedRoute)
            {
                var quantity = int.TryParse(QuantityTextBox.Text, out int qty) ? qty : 1;
                var paymentMethod = (PaymentStrategyComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? "Cash";
                
                // Strategy Pattern - отримання стратегії оплати
                IPaymentStrategy paymentStrategy = paymentMethod switch
                {
                    "Card" => new CardPaymentStrategy(),
                    "Online" => new OnlinePaymentStrategy(),
                    _ => new CashPaymentStrategy()
                };
                
                decimal totalPrice = 0;

                if (quantity > 1 && _passengerTickets.Count > 0)
                {
                    // Розрахунок для кількох квитків з різними типами та пасажирами
                    foreach (var passengerTicket in _passengerTickets)
                    {
                        var ticketType = passengerTicket.GetTicketType();
                        var factory = TicketFactoryProvider.GetFactory(ticketType, paymentStrategy);
                        var tempTicket = factory.CreateTicket(selectedRoute, new Passenger(), DateTime.Now, selectedRoute.BasePrice);
                        
                        // Decorator Pattern - додавання додаткових послуг
                        if (InsuranceCheckBox.IsChecked == true)
                        {
                            var insuranceDecorator = new InsuranceDecorator();
                            tempTicket = insuranceDecorator.Decorate(tempTicket);
                        }
                        if (BaggageCheckBox.IsChecked == true)
                        {
                            var baggageDecorator = new BaggageDecorator();
                            tempTicket = baggageDecorator.Decorate(tempTicket);
                        }
                        
                        totalPrice += tempTicket.Price;
                    }
                }
                else
                {
                    // Розрахунок для одного квитка
                    var ticketType = (TicketTypeComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? "Regular";
                    var factory = TicketFactoryProvider.GetFactory(ticketType, paymentStrategy);
                    var tempTicket = factory.CreateTicket(selectedRoute, new Passenger(), DateTime.Now, selectedRoute.BasePrice);
                    
                    // Decorator Pattern - додавання додаткових послуг
                    if (InsuranceCheckBox.IsChecked == true)
                    {
                        var insuranceDecorator = new InsuranceDecorator();
                        tempTicket = insuranceDecorator.Decorate(tempTicket);
                    }
                    if (BaggageCheckBox.IsChecked == true)
                    {
                        var baggageDecorator = new BaggageDecorator();
                        tempTicket = baggageDecorator.Decorate(tempTicket);
                    }
                    
                    totalPrice = tempTicket.Price;
                }
                
                TotalPriceTextBlock.Text = $"{totalPrice:C}";
            }
        }

        private void BuyTicketButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RoutesComboBox.SelectedItem is not Route selectedRoute)
                {
                    MessageBox.Show("Виберіть маршрут!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var quantity = int.TryParse(QuantityTextBox.Text, out int qty) ? qty : 1;
                if (quantity < 1 || quantity > 10)
                {
                    MessageBox.Show("Кількість квитків повинна бути від 1 до 10", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Валідація даних залежно від кількості квитків
                if (quantity == 1)
                {
                    // Для одного квитка перевіряємо поля для одного пасажира
                    if (string.IsNullOrWhiteSpace(PassengerLastNameTextBox.Text) || 
                        string.IsNullOrWhiteSpace(PassengerFirstNameTextBox.Text))
                    {
                        MessageBox.Show("Введіть прізвище та ім'я пасажира!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    // Для кількох квитків перевіряємо, чи є дані в _passengerTickets
                    if (_passengerTickets.Count == 0)
                    {
                        MessageBox.Show("Введіть дані для всіх пасажирів!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                
                var paymentMethod = (PaymentStrategyComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? "Cash";
                var date = DatePicker.SelectedDate ?? DateTime.Now;

                // Strategy Pattern - отримання стратегії оплати
                IPaymentStrategy paymentStrategy = paymentMethod switch
                {
                    "Card" => new CardPaymentStrategy(),
                    "Online" => new OnlinePaymentStrategy(),
                    _ => new CashPaymentStrategy()
                };

                // Template Method Pattern - обробка квитків
                var processor = new StandardTicketProcessor();

                // Створення квитків з різними типами та пасажирами
                if (quantity > 1)
                {
                    // Створення квитків з різними типами та пасажирами
                    foreach (var passengerTicket in _passengerTickets)
                    {
                        // Валідація даних пасажира
                        if (string.IsNullOrWhiteSpace(passengerTicket.PassengerLastName) || 
                            string.IsNullOrWhiteSpace(passengerTicket.PassengerFirstName))
                        {
                            MessageBox.Show($"Введіть прізвище та ім'я для квитка {passengerTicket.TicketNumber}!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var ticketType = passengerTicket.GetTicketType();
                        var factory = TicketFactoryProvider.GetFactory(ticketType, paymentStrategy);
                        
                        // Створення пасажира для цього квитка
                        var ticketPassenger = new Passenger
                        {
                            LastName = passengerTicket.PassengerLastName,
                            FirstName = passengerTicket.PassengerFirstName,
                            MiddleName = passengerTicket.PassengerMiddleName,
                            Phone = passengerTicket.PassengerPhone,
                            Document = passengerTicket.PassengerDocument
                        };
                        
                        // Factory Pattern - створення квитка
                        var ticket = factory.CreateTicket(selectedRoute, ticketPassenger, date, selectedRoute.BasePrice);
                        
                        // Template Method Pattern - обробка квитка
                        processor.ProcessTicket(ticket);
                        
                        // Decorator Pattern - додавання додаткових послуг
                        if (InsuranceCheckBox.IsChecked == true)
                        {
                            var insuranceDecorator = new InsuranceDecorator();
                            ticket = insuranceDecorator.Decorate(ticket);
                        }
                        if (BaggageCheckBox.IsChecked == true)
                        {
                            var baggageDecorator = new BaggageDecorator();
                            ticket = baggageDecorator.Decorate(ticket);
                        }
                        
                        // Builder Pattern
                        _ticketBuilder.Reset();
                        var finalTicket = _ticketBuilder
                            .SetRoute(ticket.Route)
                            .SetPassenger(ticket.Passenger)
                            .SetDate(ticket.Date)
                            .SetPrice(ticket.Price)
                            .SetTicketType(ticket.TicketType)
                            .SetPaymentMethod(ticket.PaymentMethod)
                            .SetStatus(ticket.Status)
                            .Build();
                        
                        // State Pattern
                        var stateContext = new TicketStateContext();
                        finalTicket.Status = stateContext.GetStatus();
                        
                        // Command Pattern
                        var buyCommand = new BuyTicketCommand(_repository, finalTicket);
                        buyCommand.Execute();
                        _lastCommand = buyCommand;
                        
                        // Observer Pattern
                        _ticketSubject.NotifyTicketCreated(finalTicket);
                    }
                }
                else
                {
                    // Створення одного квитка
                    // Створення пасажира для одного квитка
                    var passenger = new Passenger
                    {
                        LastName = PassengerLastNameTextBox.Text,
                        FirstName = PassengerFirstNameTextBox.Text,
                        MiddleName = PassengerMiddleNameTextBox.Text,
                        Phone = PassengerPhoneTextBox.Text,
                        Document = PassengerDocumentTextBox.Text
                    };

                    var ticketType = (TicketTypeComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? "Regular";
                    var factory = TicketFactoryProvider.GetFactory(ticketType, paymentStrategy);
                    
                    // Factory Pattern - створення квитка
                    var ticket = factory.CreateTicket(selectedRoute, passenger, date, selectedRoute.BasePrice);
                    
                    // Template Method Pattern - обробка квитка
                    processor.ProcessTicket(ticket);
                    
                    // Decorator Pattern - додавання додаткових послуг
                    if (InsuranceCheckBox.IsChecked == true)
                    {
                        var insuranceDecorator = new InsuranceDecorator();
                        ticket = insuranceDecorator.Decorate(ticket);
                    }
                    if (BaggageCheckBox.IsChecked == true)
                    {
                        var baggageDecorator = new BaggageDecorator();
                        ticket = baggageDecorator.Decorate(ticket);
                    }
                    
                    // Builder Pattern
                    _ticketBuilder.Reset();
                    var finalTicket = _ticketBuilder
                        .SetRoute(ticket.Route)
                        .SetPassenger(ticket.Passenger)
                        .SetDate(ticket.Date)
                        .SetPrice(ticket.Price)
                        .SetTicketType(ticket.TicketType)
                        .SetPaymentMethod(ticket.PaymentMethod)
                        .SetStatus(ticket.Status)
                        .Build();
                    
                    // State Pattern
                    var stateContext = new TicketStateContext();
                    finalTicket.Status = stateContext.GetStatus();
                    
                    // Command Pattern
                    var buyCommand = new BuyTicketCommand(_repository, finalTicket);
                    buyCommand.Execute();
                    _lastCommand = buyCommand;
                    
                    // Observer Pattern
                    _ticketSubject.NotifyTicketCreated(finalTicket);
                }

                MessageBox.Show($"Успішно придбано {quantity} квитків!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Очищення всіх полів
                ClearAllFields();
                
                // Оновлення списку квитків та перехід на вкладку "Список квитків"
                RefreshTickets();
                MainTabControl.SelectedIndex = 1; // Перехід на вкладку "Список квитків"
                
                StatusTextBlock.Text = $"Створено {quantity} квитків";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = $"Помилка: {ex.Message}";
            }
        }

        private void RefreshTicketsButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshTickets();
        }

        private void ClearAllFields()
        {
            // Очищення полів пасажира
            PassengerLastNameTextBox.Clear();
            PassengerFirstNameTextBox.Clear();
            PassengerMiddleNameTextBox.Clear();
            PassengerPhoneTextBox.Clear();
            PassengerDocumentTextBox.Clear();
            
            // Очищення полів маршруту та інших
            RoutesComboBox.SelectedItem = null;
            DatePicker.SelectedDate = DateTime.Now;
            QuantityTextBox.Text = "1";
            TicketTypeComboBox.SelectedIndex = 0;
            PaymentStrategyComboBox.SelectedIndex = 0;
            InsuranceCheckBox.IsChecked = false;
            BaggageCheckBox.IsChecked = false;
            TotalPriceTextBlock.Text = "";
            
            // Очищення множинних квитків
            _passengerTickets.Clear();
            MultiplePassengersItemsControl.ItemsSource = null;
            SinglePassengerPanel.Visibility = Visibility.Visible;
            MultiplePassengersItemsControl.Visibility = Visibility.Collapsed;
        }

        private void ExportToPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (TicketsDataGrid == null)
            {
                MessageBox.Show("Помилка: DataGrid не ініціалізовано!", "Помилка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (TicketsDataGrid.SelectedItem is Ticket selectedTicket)
            {
                // Додаткова перевірка на null
                if (selectedTicket == null)
                {
                    MessageBox.Show("Помилка: Обраний квиток некоректний!", "Помилка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // Запитуємо формат експорту
                var formatResult = MessageBox.Show(
                    "Оберіть формат експорту квитка:\n\n" +
                    "Так - Експортувати в PDF\n" +
                    "Ні - Експортувати в текстовий файл\n" +
                    "Скасувати - Відмінити експорт",
                    "Вибір формату експорту",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                
                if (formatResult == MessageBoxResult.Cancel)
                {
                    return;
                }
                
                bool exportAsPdf = formatResult == MessageBoxResult.Yes;
                
                try
                {
                    string fileName;
                    string formatName = exportAsPdf ? "PDF" : "текстовий файл";
                    
                if (exportAsPdf)
                {
                    System.Diagnostics.Debug.WriteLine("Генерація PDF квитка через LaTeX...");
                    var latexGenerator = new LatexTicketGenerator();
                    fileName = latexGenerator.GenerateTicket(selectedTicket);
                    System.Diagnostics.Debug.WriteLine($"PDF згенеровано: {fileName}");
                }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Генерація текстового файлу квитка...");
                        var textGenerator = new TextTicketGenerator();
                        fileName = textGenerator.GenerateTicket(selectedTicket);
                        System.Diagnostics.Debug.WriteLine($"Текстовий файл згенеровано: {fileName}");
                    }
                    System.Diagnostics.Debug.WriteLine($"Текстовий файл згенеровано: {fileName}");
                    
                    // Перевіряємо, чи файл існує
                    if (!System.IO.File.Exists(fileName))
                    {
                        throw new System.IO.FileNotFoundException($"{formatName} не було створено: {fileName}");
                    }
                    
                    // Відкриваємо файл
                    try
                    {
                        var processInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = fileName,
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(processInfo);
                    }
                    catch (Exception openEx)
                    {
                        // Якщо не вдалося відкрити, показуємо повідомлення
                        System.Diagnostics.Debug.WriteLine($"Помилка відкриття файлу: {openEx.Message}");
                        MessageBox.Show($"{formatName} створено, але не вдалося відкрити автоматично.\n\nФайл: {fileName}", 
                            "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    
                    MessageBox.Show($"Квиток успішно експортовано в {formatName} та відкрито!", "Успіх", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    if (StatusTextBlock != null)
                    {
                        StatusTextBlock.Text = $"Квиток експортовано в {formatName}: {System.IO.Path.GetFileName(fileName)}";
                    }
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}\n\nПереконайтеся, що файл може бути створено.", "Помилка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    if (StatusTextBlock != null)
                    {
                        StatusTextBlock.Text = $"Помилка експорту: {ex.Message}";
                    }
                    System.Diagnostics.Debug.WriteLine($"FileNotFoundException: {ex.Message}\n{ex.StackTrace}");
                }
                catch (System.IO.IOException ioEx)
                {
                    var errorMessage = $"Помилка збереження файлу:\n{ioEx.Message}";
                    if (ioEx.InnerException != null)
                    {
                        errorMessage += $"\n\nДеталі: {ioEx.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "Помилка збереження", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    if (StatusTextBlock != null)
                    {
                        StatusTextBlock.Text = $"Помилка збереження: {ioEx.Message}";
                    }
                    System.Diagnostics.Debug.WriteLine($"IOException при експорті: {ioEx.Message}");
                }
                catch (UnauthorizedAccessException accessEx)
                {
                    MessageBox.Show($"Немає доступу до папки для збереження:\n{accessEx.Message}\n\nПеревірте права доступу до папки програми.", "Помилка доступу", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    if (StatusTextBlock != null)
                    {
                        StatusTextBlock.Text = "Помилка доступу до папки";
                    }
                    System.Diagnostics.Debug.WriteLine($"UnauthorizedAccessException: {accessEx.Message}");
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Помилка експорту: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nВнутрішня помилка: {ex.InnerException.Message}";
                    }
                    errorMessage += $"\n\nТип помилки: {ex.GetType().Name}";
                    MessageBox.Show(errorMessage, "Помилка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    if (StatusTextBlock != null)
                    {
                        StatusTextBlock.Text = $"Помилка експорту: {ex.Message}";
                    }
                    System.Diagnostics.Debug.WriteLine($"Загальна помилка при експорті: {ex.GetType().Name} - {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                MessageBox.Show("Виберіть квиток для експорту!", "Помилка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RefreshTickets()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== ПОЧАТОК RefreshTickets ===");
                
                if (_repository == null)
                {
                    System.Diagnostics.Debug.WriteLine("Репозиторій null!");
                    MessageBox.Show("Репозиторій не ініціалізовано!", "Помилка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                var tickets = _repository.GetAllTickets();
                
                System.Diagnostics.Debug.WriteLine($"Отримано {tickets?.Count ?? 0} квитків з репозиторію");
                
                if (tickets == null)
                {
                    System.Diagnostics.Debug.WriteLine("Список квитків null!");
                    tickets = new List<Ticket>();
                }
                
                if (tickets.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("УВАГА: Список квитків порожній!");
                }
                
                // Visitor Pattern - обробка квитків для експорту/серіалізації
                var visitor = new JsonSerializationVisitor();
                foreach (var ticket in tickets)
                {
                    if (ticket != null)
                    {
                        visitor.Visit(ticket);
                        if (ticket.Route != null)
                            visitor.Visit(ticket.Route);
                        if (ticket.Passenger != null)
                            visitor.Visit(ticket.Passenger);
                    }
                }
                // Можна використати visitor.GetJson() для експорту
                
                // Зберігаємо всі квитки для фільтрації
                _allTickets = tickets;
                
                // Застосовуємо фільтр пошуку, якщо він активний
                var filteredTickets = ApplySearchFilter(tickets);
                
                // Оновлення DataGrid з даними з JSON
                if (TicketsDataGrid != null)
                {
                    TicketsDataGrid.ItemsSource = null; // Спочатку очищаємо
                    TicketsDataGrid.ItemsSource = filteredTickets; // Потім встановлюємо нові дані
                    
                    // Оновлюємо ComboBox для статусів
                    InitializeStatusComboBox();
                    
                    System.Diagnostics.Debug.WriteLine($"Встановлено {filteredTickets.Count} квитків в DataGrid (з {tickets.Count} загалом)");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("TicketsDataGrid null!");
                }
                
                if (StatusTextBlock != null)
                {
                    if (filteredTickets.Count < tickets.Count)
                    {
                        StatusTextBlock.Text = $"Знайдено {filteredTickets.Count} квитків з {tickets.Count} (фільтр активний)";
                    }
                    else
                    {
                        StatusTextBlock.Text = $"Завантажено {tickets.Count} квитків з JSON файлу";
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("StatusTextBlock null!");
                }
                
                System.Diagnostics.Debug.WriteLine($"=== КІНЕЦЬ RefreshTickets: {tickets.Count} квитків ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка завантаження квитків: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Внутрішня помилка: {ex.InnerException.Message}");
                }
                MessageBox.Show($"Помилка завантаження квитків: {ex.Message}\n\nДеталі: {ex.StackTrace}", "Помилка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                if (StatusTextBlock != null)
                {
                    StatusTextBlock.Text = $"Помилка завантаження: {ex.Message}";
                }
            }
        }

        private void CancelTicketButton_Click(object sender, RoutedEventArgs e)
        {
            if (TicketsDataGrid.SelectedItem is Ticket selectedTicket)
            {
                try
                {
                    // State Pattern - перевірка стану перед скасуванням
                    var stateContext = new TicketStateContext();
                    if (selectedTicket.Status == "Used")
                    {
                        MessageBox.Show("Не можна скасувати використаний квиток!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    // Command Pattern - виконання команди скасування
                    var cancelCommand = new CancelTicketCommand(_repository, selectedTicket.Id);
                    cancelCommand.Execute();
                    _lastCommand = cancelCommand;
                    
                    // State Pattern - зміна стану на скасований
                    stateContext.Cancel();
                    selectedTicket.Status = stateContext.GetStatus();
                    
                    // Observer Pattern - сповіщення про скасування
                    _ticketSubject.NotifyTicketCancelled(selectedTicket);
                    
                    // Зберігаємо зміни
                    _repository.SaveAll();
                    
                    MessageBox.Show("Квиток скасовано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshTickets();
                    StatusTextBlock.Text = $"Квиток #{selectedTicket.Id} скасовано";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Виберіть квиток для скасування!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TicketsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Можна додати додаткову логіку при виборі квитка
        }

        private void TicketsDataGrid_BeginningEdit(object sender, System.Windows.Controls.DataGridBeginningEditEventArgs e)
        {
            // Встановлюємо ItemsSource для ComboBox перед редагуванням
            if (e.Column.Header?.ToString() == "Статус" && e.Column is System.Windows.Controls.DataGridComboBoxColumn comboColumn)
            {
                if (comboColumn.ItemsSource == null)
                {
                    comboColumn.ItemsSource = StatusList;
                }
            }
        }

        private void TicketsDataGrid_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            // Обробка зміни статусу квитка
            if (e.Column.Header?.ToString() == "Статус" && e.Row.Item is Ticket ticket)
            {
                try
                {
                    var comboBox = e.EditingElement as System.Windows.Controls.ComboBox;
                    if (comboBox == null) return;
                    
                    // Отримуємо новий статус з ComboBox
                    string? newStatus = null;
                    if (comboBox.SelectedItem is string selectedString)
                    {
                        newStatus = selectedString;
                    }
                    else
                    {
                        newStatus = comboBox.SelectedItem?.ToString();
                    }
                    
                    if (string.IsNullOrEmpty(newStatus))
                    {
                        // Якщо не вдалося отримати з SelectedItem, спробуємо з Text
                        newStatus = comboBox.Text;
                    }
                    
                    if (string.IsNullOrEmpty(newStatus) || newStatus == ticket.Status)
                    {
                        // Статус не змінився або порожній
                        return;
                    }

                    // Використовуємо State Pattern для зміни статусу
                    var stateContext = new TicketStateContext();
                    
                    // Встановлюємо поточний стан на основі поточного статусу
                    switch (ticket.Status)
                    {
                        case "Active":
                            stateContext.SetState(new ActiveTicketState());
                            break;
                        case "Cancelled":
                            stateContext.SetState(new CancelledTicketState());
                            break;
                        case "Used":
                            stateContext.SetState(new UsedTicketState());
                            break;
                    }

                    // Змінюємо статус через State Pattern
                    switch (newStatus)
                    {
                        case "Active":
                            if (ticket.Status == "Cancelled")
                            {
                                MessageBox.Show("Не можна активувати скасований квиток!", "Помилка", 
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                                e.Cancel = true;
                                return;
                            }
                            if (ticket.Status == "Used")
                            {
                                MessageBox.Show("Не можна активувати використаний квиток!", "Помилка", 
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                                e.Cancel = true;
                                return;
                            }
                            stateContext.Activate();
                            break;
                        case "Cancelled":
                            if (ticket.Status == "Used")
                            {
                                MessageBox.Show("Не можна скасувати використаний квиток!", "Помилка", 
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                                e.Cancel = true;
                                return;
                            }
                            stateContext.Cancel();
                            break;
                        case "Used":
                            if (ticket.Status == "Cancelled")
                            {
                                MessageBox.Show("Не можна використати скасований квиток!", "Помилка", 
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                                e.Cancel = true;
                                return;
                            }
                            stateContext.Use();
                            break;
                    }

                    // Зберігаємо старий статус перед зміною
                    var oldStatus = ticket.Status;
                    
                    // Оновлюємо статус квитка
                    ticket.Status = newStatus;
                    
                    // Оновлюємо квиток в репозиторії
                    _repository.UpdateTicket(ticket);
                    
                    // Зберігаємо зміни
                    _repository.SaveAll();
                    
                    // Сповіщаємо спостерігачів (Observer Pattern)
                    _ticketSubject.NotifyTicketStatusChanged(ticket, oldStatus, newStatus);
                    
                    // Оновлюємо відображення
                    RefreshTickets();
                    
                    if (StatusTextBlock != null)
                    {
                        StatusTextBlock.Text = $"Статус квитка #{ticket.Id} змінено на: {GetStatusDisplayName(newStatus)}";
                    }
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Помилка зміни статусу", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    e.Cancel = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка зміни статусу: {ex.Message}", "Помилка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                }
            }
        }

        private string GetStatusDisplayName(string status)
        {
            return status switch
            {
                "Active" => "Активний",
                "Cancelled" => "Скасований",
                "Used" => "Використаний",
                _ => status
            };
        }

        private void ExportToPdfByIdButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TicketIdTextBox?.Text))
            {
                MessageBox.Show("Введіть ID квитка!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TicketIdTextBox.Text, out int ticketId))
            {
                MessageBox.Show("ID квитка повинен бути числом!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Перезавантажуємо дані з JSON перед пошуком квитка
                System.Diagnostics.Debug.WriteLine($"Пошук квитка з ID: {ticketId}");
                _repository.LoadAll();
                
                var ticket = _repository.GetTicket(ticketId);
                if (ticket == null)
                {
                    MessageBox.Show($"Квиток з ID {ticketId} не знайдено в базі даних (JSON файл)!\n\nПеревірте, чи існує квиток з таким ID.", "Помилка", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Квиток знайдено: ID={ticket.Id}, Route={ticket.Route?.DisplayName ?? "null"}, Passenger={ticket.Passenger?.FullName ?? "null"}, Date={ticket.Date:yyyy-MM-dd}");

                // Запитуємо формат експорту
                var formatResult = MessageBox.Show(
                    "Оберіть формат експорту квитка:\n\n" +
                    "Так - Експортувати в PDF\n" +
                    "Ні - Експортувати в текстовий файл\n" +
                    "Скасувати - Відмінити експорт",
                    "Вибір формату експорту",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                
                if (formatResult == MessageBoxResult.Cancel)
                {
                    return;
                }
                
                bool exportAsPdf = formatResult == MessageBoxResult.Yes;
                
                string fileName;
                string formatName = exportAsPdf ? "PDF" : "текстовий файл";
                
                if (exportAsPdf)
                {
                    System.Diagnostics.Debug.WriteLine("Генерація PDF квитка через LaTeX...");
                    var latexGenerator = new LatexTicketGenerator();
                    fileName = latexGenerator.GenerateTicket(ticket);
                    System.Diagnostics.Debug.WriteLine($"PDF згенеровано: {fileName}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Генерація текстового файлу квитка...");
                    var textGenerator = new TextTicketGenerator();
                    fileName = textGenerator.GenerateTicket(ticket);
                    System.Diagnostics.Debug.WriteLine($"Текстовий файл згенеровано: {fileName}");
                }
                
                // Перевіряємо, чи файл існує
                if (!System.IO.File.Exists(fileName))
                {
                    throw new System.IO.FileNotFoundException($"Файл не було створено: {fileName}");
                }
                
                // Відкриваємо файл
                try
                {
                    var processInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = fileName,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(processInfo);
                }
                catch (Exception openEx)
                {
                    // Якщо не вдалося відкрити, показуємо повідомлення
                    System.Diagnostics.Debug.WriteLine($"Помилка відкриття файлу: {openEx.Message}");
                    MessageBox.Show($"{formatName} створено, але не вдалося відкрити автоматично.\n\nФайл: {fileName}", 
                        "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
                MessageBox.Show($"Квиток #{ticketId} успішно експортовано в {formatName} та відкрито!", "Успіх", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                if (StatusTextBlock != null)
                {
                    StatusTextBlock.Text = $"Квиток #{ticketId} експортовано в {formatName}: {System.IO.Path.GetFileName(fileName)}";
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}\n\nПереконайтеся, що файл може бути створено.", "Помилка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                if (StatusTextBlock != null)
                {
                    StatusTextBlock.Text = $"Помилка експорту: {ex.Message}";
                }
                System.Diagnostics.Debug.WriteLine($"FileNotFoundException: {ex.Message}\n{ex.StackTrace}");
            }
            catch (System.IO.IOException ioEx)
            {
                var errorMessage = $"Помилка збереження файлу:\n{ioEx.Message}";
                if (ioEx.InnerException != null)
                {
                    errorMessage += $"\n\nДеталі: {ioEx.InnerException.Message}";
                }
                MessageBox.Show(errorMessage, "Помилка збереження", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                if (StatusTextBlock != null)
                {
                    StatusTextBlock.Text = $"Помилка збереження текстового файлу: {ioEx.Message}";
                }
                System.Diagnostics.Debug.WriteLine($"IOException: {ioEx.Message}\n{ioEx.StackTrace}");
            }
            catch (UnauthorizedAccessException accessEx)
            {
                MessageBox.Show($"Немає доступу до папки для збереження:\n{accessEx.Message}\n\nПеревірте права доступу до папки програми.", "Помилка доступу", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                if (StatusTextBlock != null)
                {
                    StatusTextBlock.Text = $"Помилка доступу: {accessEx.Message}";
                }
                System.Diagnostics.Debug.WriteLine($"UnauthorizedAccessException: {accessEx.Message}\n{accessEx.StackTrace}");
            }
            catch (Exception ex)
            {
                var errorMessage = $"Помилка експорту в текстовий файл: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nДеталі: {ex.InnerException.Message}";
                }
                errorMessage += $"\n\nТип помилки: {ex.GetType().Name}";
                MessageBox.Show(errorMessage, "Помилка експорту", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                if (StatusTextBlock != null)
                {
                    StatusTextBlock.Text = $"Помилка експорту: {ex.Message}";
                }
                System.Diagnostics.Debug.WriteLine($"Exception in ExportToPdfByIdButton_Click: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"InnerException: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                }
            }
        }

        private void CancelTicketByIdButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TicketIdTextBox?.Text))
            {
                MessageBox.Show("Введіть ID квитка!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TicketIdTextBox.Text, out int ticketId))
            {
                MessageBox.Show("ID квитка повинен бути числом!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var ticket = _repository.GetTicket(ticketId);
                if (ticket == null)
                {
                    MessageBox.Show($"Квиток з ID {ticketId} не знайдено!", "Помилка", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // State Pattern - перевірка стану перед скасуванням
                var stateContext = new TicketStateContext();
                if (ticket.Status == "Used")
                {
                    MessageBox.Show("Не можна скасувати використаний квиток!", "Помилка", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Command Pattern - виконання команди скасування
                var cancelCommand = new CancelTicketCommand(_repository, ticketId);
                cancelCommand.Execute();
                _lastCommand = cancelCommand;
                
                // State Pattern - зміна стану на скасований
                stateContext.Cancel();
                ticket.Status = stateContext.GetStatus();
                
                // Observer Pattern - сповіщення про скасування
                _ticketSubject.NotifyTicketCancelled(ticket);
                
                // Зберігаємо зміни
                _repository.SaveAll();
                
                MessageBox.Show("Квиток скасовано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshTickets();
                StatusTextBlock.Text = $"Квиток #{ticketId} скасовано";
                TicketIdTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteTicketByIdButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TicketIdTextBox?.Text))
            {
                MessageBox.Show("Введіть ID квитка!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TicketIdTextBox.Text, out int ticketId))
            {
                MessageBox.Show("ID квитка повинен бути числом!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var ticket = _repository.GetTicket(ticketId);
                if (ticket == null)
                {
                    MessageBox.Show($"Квиток з ID {ticketId} не знайдено!", "Помилка", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Ви впевнені, що хочете видалити квиток #{ticketId}?\n\n" +
                    $"Маршрут: {ticket.RouteInfo}\n" +
                    $"Пасажир: {ticket.PassengerName}\n" +
                    $"Ця операція незворотна!",
                    "Підтвердження видалення",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _repository.RemoveTicket(ticketId);
                    _repository.SaveAll();
                    
                    MessageBox.Show("Квиток видалено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshTickets();
                    StatusTextBlock.Text = $"Квиток #{ticketId} видалено";
                    TicketIdTextBox.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка видалення: {ex.Message}", "Помилка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Alt+F4 для закриття вікна
            if (e.Key == System.Windows.Input.Key.F4 && 
                (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Alt) == System.Windows.Input.ModifierKeys.Alt)
            {
                Application.Current.Shutdown();
            }
        }

        private List<Ticket> ApplySearchFilter(List<Ticket> tickets)
        {
            if (string.IsNullOrWhiteSpace(SearchLastNameTextBox?.Text))
            {
                return tickets;
            }

            string searchText = SearchLastNameTextBox.Text.Trim().ToLower();
            return tickets.Where(ticket =>
            {
                if (ticket.Passenger == null)
                    return false;

                string lastName = ticket.Passenger.LastName ?? "";
                return lastName.ToLower().Contains(searchText);
            }).ToList();
        }

        private void SearchLastNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Застосовуємо фільтр при зміні тексту пошуку
            if (TicketsDataGrid != null && _allTickets != null && _allTickets.Count > 0)
            {
                var filteredTickets = ApplySearchFilter(_allTickets);
                TicketsDataGrid.ItemsSource = null;
                TicketsDataGrid.ItemsSource = filteredTickets;

                if (StatusTextBlock != null)
                {
                    if (filteredTickets.Count < _allTickets.Count)
                    {
                        StatusTextBlock.Text = $"Знайдено {filteredTickets.Count} квитків з {_allTickets.Count} (фільтр активний)";
                    }
                    else
                    {
                        StatusTextBlock.Text = $"Завантажено {_allTickets.Count} квитків з JSON файлу";
                    }
                }
            }
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchLastNameTextBox != null)
            {
                SearchLastNameTextBox.Clear();
            }
            // Оновлюємо відображення (фільтр буде застосовано автоматично через TextChanged)
            RefreshTickets();
        }
    }
}

