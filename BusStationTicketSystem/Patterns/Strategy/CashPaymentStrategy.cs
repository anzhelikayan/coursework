namespace BusStationTicketSystem.Patterns.Strategy
{
    public class CashPaymentStrategy : IPaymentStrategy
    {
        public decimal CalculatePrice(decimal basePrice)
        {
            return basePrice;
        }

        public string GetPaymentMethod() => "Cash";
    }
}


