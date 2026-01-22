using System;
using BusStationTicketSystem.Models;
using BusStationTicketSystem.Patterns.Strategy;

namespace BusStationTicketSystem.Patterns.Factory
{
    public class ChildTicketFactory : ITicketFactory
    {
        private readonly IPaymentStrategy _paymentStrategy;

        public ChildTicketFactory(IPaymentStrategy paymentStrategy)
        {
            _paymentStrategy = paymentStrategy;
        }

        public Ticket CreateTicket(Route route, Passenger passenger, DateTime date, decimal basePrice)
        {
            var childPrice = basePrice * 0.5m;
            return new Ticket
            {
                Route = route,
                Passenger = passenger,
                Date = date,
                Price = _paymentStrategy.CalculatePrice(childPrice),
                TicketType = "Child",
                PaymentMethod = _paymentStrategy.GetPaymentMethod(),
                Status = "Active"
            };
        }
    }
}

