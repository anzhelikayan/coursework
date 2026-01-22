using BusStationTicketSystem.Models;

namespace BusStationTicketSystem.Patterns.Decorator
{
    public class BaggageDecorator : ITicketDecorator
    {
        private const decimal BaggagePrice = 30m;

        public Ticket Decorate(Ticket ticket)
        {
            // Створюємо новий квиток з додатковою ціною багажу
            var decoratedTicket = new Ticket
            {
                Id = ticket.Id,
                Route = ticket.Route,
                Passenger = ticket.Passenger,
                Date = ticket.Date,
                Price = ticket.Price + BaggagePrice,
                TicketType = ticket.TicketType,
                PaymentMethod = ticket.PaymentMethod,
                Status = ticket.Status
            };

            return decoratedTicket;
        }
    }
}
