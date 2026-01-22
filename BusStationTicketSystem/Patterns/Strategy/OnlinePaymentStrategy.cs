namespace BusStationTicketSystem.Patterns.Strategy
{
    public class OnlinePaymentStrategy : IPaymentStrategy
    {
        public decimal CalculatePrice(decimal basePrice)
        {
            return basePrice * 0.95m;
        }

        public string GetPaymentMethod() => "Online";
    }
}


