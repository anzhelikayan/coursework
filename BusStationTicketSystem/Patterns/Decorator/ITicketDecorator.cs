using BusStationTicketSystem.Models;

namespace BusStationTicketSystem.Patterns.Decorator
{
    public interface ITicketDecorator
    {
        Ticket Decorate(Ticket ticket);
    }
}
