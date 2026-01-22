using System;

namespace BusStationTicketSystem.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public Route Route { get; set; } = null!;
        public Passenger Passenger { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public string TicketType { get; set; } = "Regular";
        public string PaymentMethod { get; set; } = "Cash";
        public string Status { get; set; } = "Active";
        public string RouteInfo => $"{Route?.Departure} → {Route?.Arrival}";
        public string PassengerName => Passenger?.FullName ?? "";
    }
}

