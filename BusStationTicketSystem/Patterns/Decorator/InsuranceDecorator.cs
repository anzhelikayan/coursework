using BusStationTicketSystem.Models;

namespace BusStationTicketSystem.Patterns.Decorator
{
    public class InsuranceDecorator : ITicketDecorator
    {
        private const decimal InsurancePrice = 50m;

        public Ticket Decorate(Ticket ticket)
        {
            // Створюємо новий квиток з додатковою ціною страхування
            var decoratedTicket = new Ticket
            {
                Id = ticket.Id,
                Route = ticket.Route,
                Passenger = ticket.Passenger,
                Date = ticket.Date,
                Price = ticket.Price + InsurancePrice,
                TicketType = ticket.TicketType,
                PaymentMethod = ticket.PaymentMethod,
                Status = ticket.Status
            };

            return decoratedTicket;
        }
    }
}
