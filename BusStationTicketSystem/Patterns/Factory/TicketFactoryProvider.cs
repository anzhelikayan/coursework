using BusStationTicketSystem.Patterns.Strategy;

namespace BusStationTicketSystem.Patterns.Factory
{
    public class TicketFactoryProvider
    {
        public static ITicketFactory GetFactory(string ticketType, IPaymentStrategy paymentStrategy)
        {
            return ticketType switch
            {
                "Regular" => new RegularTicketFactory(paymentStrategy),
                "Discount" => new DiscountTicketFactory(paymentStrategy),
                "Child" => new ChildTicketFactory(paymentStrategy),
                _ => new RegularTicketFactory(paymentStrategy)
            };
        }
    }
}


