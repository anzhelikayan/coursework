using System.Collections.Generic;
using System.Text;
using BusStationTicketSystem.Models;

namespace BusStationTicketSystem.Patterns.Visitor
{
    public class JsonSerializationVisitor : ITicketVisitor
    {
        private readonly StringBuilder _jsonBuilder = new StringBuilder();
        private readonly List<string> _tickets = new List<string>();
        private readonly List<string> _routes = new List<string>();
        private readonly List<string> _passengers = new List<string>();

        public void Visit(Ticket ticket)
        {
            var json = $@"{{
    ""Id"": {ticket.Id},
    ""RouteId"": {ticket.Route.Id},
    ""PassengerName"": ""{ticket.Passenger.FullName}"",
    ""PassengerPhone"": ""{ticket.Passenger.Phone}"",
    ""PassengerDocument"": ""{ticket.Passenger.Document}"",
    ""Date"": ""{ticket.Date:yyyy-MM-ddTHH:mm:ss}"",
    ""Price"": {ticket.Price},
    ""TicketType"": ""{ticket.TicketType}"",
    ""PaymentMethod"": ""{ticket.PaymentMethod}"",
    ""Status"": ""{ticket.Status}""
}}";
            _tickets.Add(json);
        }

        public void Visit(Route route)
        {
            // Безпечне форматування TimeSpan
            string departureTimeStr = route.DepartureTime.ToString(@"hh\:mm\:ss");
            
            var json = $@"{{
    ""Id"": {route.Id},
    ""Departure"": ""{route.Departure}"",
    ""Arrival"": ""{route.Arrival}"",
    ""Distance"": {route.Distance},
    ""BasePrice"": {route.BasePrice},
    ""DepartureTime"": ""{departureTimeStr}""
}}";
            _routes.Add(json);
        }

        public void Visit(Passenger passenger)
        {
            var json = $@"{{
    ""Name"": ""{passenger.FullName}"",
    ""Phone"": ""{passenger.Phone}"",
    ""Document"": ""{passenger.Document}""
}}";
            _passengers.Add(json);
        }

        public string GetJson()
        {
            _jsonBuilder.Clear();
            _jsonBuilder.AppendLine("{");
            _jsonBuilder.AppendLine("  \"Routes\": [");
            _jsonBuilder.AppendLine(string.Join(",\n", _routes));
            _jsonBuilder.AppendLine("  ],");
            _jsonBuilder.AppendLine("  \"Tickets\": [");
            _jsonBuilder.AppendLine(string.Join(",\n", _tickets));
            _jsonBuilder.AppendLine("  ]");
            _jsonBuilder.AppendLine("}");
            return _jsonBuilder.ToString();
        }

        public void Reset()
        {
            _tickets.Clear();
            _routes.Clear();
            _passengers.Clear();
        }
    }
}



