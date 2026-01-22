using BusStationTicketSystem.Models;
using BusStationTicketSystem.Patterns.Repository;

namespace BusStationTicketSystem.Patterns.Observer
{
    public class JsonSaveObserver : ITicketObserver
    {
        private readonly ITicketRepository _repository;

        public JsonSaveObserver(ITicketRepository repository)
        {
            _repository = repository;
        }

        public void OnTicketCreated(Ticket ticket)
        {
            _repository.SaveAll();
        }

        public void OnTicketCancelled(Ticket ticket)
        {
            _repository.SaveAll();
        }

        public void OnTicketStatusChanged(Ticket ticket, string oldStatus, string newStatus)
        {
            _repository.SaveAll();
        }
    }
}
