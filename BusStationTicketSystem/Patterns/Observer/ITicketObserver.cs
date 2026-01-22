using BusStationTicketSystem.Models;

namespace BusStationTicketSystem.Patterns.Observer
{
    public interface ITicketObserver
    {
        void OnTicketCreated(Ticket ticket);
        void OnTicketCancelled(Ticket ticket);
        void OnTicketStatusChanged(Ticket ticket, string oldStatus, string newStatus);
    }
}

