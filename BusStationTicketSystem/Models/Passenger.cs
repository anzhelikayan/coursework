using System;

namespace BusStationTicketSystem.Models
{
    public class Passenger
    {
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;

        // Властивість для повного імені (для сумісності зі старим кодом)
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
        
        // Властивість Name для сумісності
        public string Name
        {
            get => FullName;
            set
            {
                // Розбиваємо повне ім'я на складові, якщо передано
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    LastName = parts.Length > 0 ? parts[0] : string.Empty;
                    FirstName = parts.Length > 1 ? parts[1] : string.Empty;
                    MiddleName = parts.Length > 2 ? parts[2] : string.Empty;
                }
            }
        }
    }
}

