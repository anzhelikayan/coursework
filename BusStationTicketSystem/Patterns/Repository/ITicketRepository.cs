using System.Collections.Generic;
using BusStationTicketSystem.Models;

namespace BusStationTicketSystem.Patterns.Repository
{
    public interface ITicketRepository
    {
        void AddTicket(Ticket ticket);
        void RemoveTicket(int ticketId);
        void UpdateTicket(Ticket ticket);
        Ticket? GetTicket(int ticketId);
        List<Ticket> GetAllTickets();
        List<Route> GetAllRoutes();
        void SaveAll();
        void LoadAll();
    }
}

