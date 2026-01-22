using BusStationTicketSystem.Models;
using BusStationTicketSystem.Patterns.Repository;

namespace BusStationTicketSystem.Patterns.Command
{
    public class CancelTicketCommand : ICommand
    {
        private readonly ITicketRepository _repository;
        private readonly int _ticketId;
        private Ticket? _cancelledTicket;
        private bool _executed = false;

        public CancelTicketCommand(ITicketRepository repository, int ticketId)
        {
            _repository = repository;
            _ticketId = ticketId;
        }

        public void Execute()
        {
            if (!_executed)
            {
                var ticket = _repository.GetTicket(_ticketId);
                if (ticket != null)
                {
                    _cancelledTicket = ticket;
                    ticket.Status = "Cancelled";
                    _repository.UpdateTicket(ticket);
                    _executed = true;
                }
            }
        }

        public void Undo()
        {
            if (_executed && _cancelledTicket != null)
            {
                _cancelledTicket.Status = "Active";
                _repository.UpdateTicket(_cancelledTicket);
                _executed = false;
            }
        }
    }
}

