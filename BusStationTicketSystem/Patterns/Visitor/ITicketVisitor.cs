using BusStationTicketSystem.Models;

namespace BusStationTicketSystem.Patterns.Visitor
{
    public interface ITicketVisitor
    {
        void Visit(Ticket ticket);
        void Visit(Route route);
        void Visit(Passenger passenger);
    }
}
