using System.Collections.Generic;

namespace BusStationTicketSystem.Models
{
    public class TicketData
    {
        public List<Route> Routes { get; set; } = new List<Route>();
        public List<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}



