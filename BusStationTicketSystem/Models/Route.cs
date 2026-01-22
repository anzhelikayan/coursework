using System;

namespace BusStationTicketSystem.Models
{
    public class Route
    {
        public int Id { get; set; }
        public string Departure { get; set; } = string.Empty;
        public string Arrival { get; set; } = string.Empty;
        public int Distance { get; set; } // в кілометрах
        public decimal BasePrice { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public string DisplayName => $"{Departure} → {Arrival}";
    }
}



