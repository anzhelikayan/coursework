using System;
using BusStationTicketSystem.Models;
using BusStationTicketSystem.Patterns.Strategy;

namespace BusStationTicketSystem.Patterns.Factory
{
    public class RegularTicketFactory : ITicketFactory
    {
        private readonly IPaymentStrategy _paymentStrategy;

        public RegularTicketFactory(IPaymentStrategy paymentStrategy)
        {
            _paymentStrategy = paymentStrategy;
        }

        public Ticket CreateTicket(Route route, Passenger passenger, DateTime date, decimal basePrice)
        {
            return new Ticket
            {
                Route = route,
                Passenger = passenger,
                Date = date,
                Price = _paymentStrategy.CalculatePrice(basePrice),
                TicketType = "Regular",
                PaymentMethod = _paymentStrategy.GetPaymentMethod(),
                Status = "Active"
            };
        }
    }
}
