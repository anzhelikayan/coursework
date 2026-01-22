using System;
using BusStationTicketSystem.Models;
using BusStationTicketSystem.Patterns.Strategy;

namespace BusStationTicketSystem.Patterns.Factory
{
    public class DiscountTicketFactory : ITicketFactory
    {
        private readonly IPaymentStrategy _paymentStrategy;

        public DiscountTicketFactory(IPaymentStrategy paymentStrategy)
        {
            _paymentStrategy = paymentStrategy;
        }

        public Ticket CreateTicket(Route route, Passenger passenger, DateTime date, decimal basePrice)
        {
            var discountedPrice = basePrice * 0.7m;
            return new Ticket
            {
                Route = route,
                Passenger = passenger,
                Date = date,
                Price = _paymentStrategy.CalculatePrice(discountedPrice),
                TicketType = "Discount",
                PaymentMethod = _paymentStrategy.GetPaymentMethod(),
                Status = "Active"
            };
        }
    }
}
