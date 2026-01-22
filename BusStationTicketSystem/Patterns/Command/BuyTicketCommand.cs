using BusStationTicketSystem.Models;
using BusStationTicketSystem.Patterns.Repository;

namespace BusStationTicketSystem.Patterns.Command
{
    public class BuyTicketCommand : ICommand
    {
        private readonly ITicketRepository _repository;
        private readonly Ticket _ticket;
        private bool _executed = false;

        public BuyTicketCommand(ITicketRepository repository, Ticket ticket)
        {
            _repository = repository;
            _ticket = ticket;
        }

        public void Execute()
        {
            if (!_executed)
            {
                _repository.AddTicket(_ticket);
                _executed = true;
            }
        }

        public void Undo()
        {
            if (_executed)
            {
                _repository.RemoveTicket(_ticket.Id);
                _executed = false;
            }
        }
    }
}

