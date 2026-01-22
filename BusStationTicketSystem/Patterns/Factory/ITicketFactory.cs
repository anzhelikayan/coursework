using System;
using BusStationTicketSystem.Models;

namespace BusStationTicketSystem.Patterns.Factory
{
    public interface ITicketFactory
    {
        Ticket CreateTicket(Route route, Passenger passenger, DateTime date, decimal basePrice);
    }
}
