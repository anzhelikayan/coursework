namespace BusStationTicketSystem.Patterns.Strategy
{
    public class CardPaymentStrategy : IPaymentStrategy
    {
        public decimal CalculatePrice(decimal basePrice)
        {
            return basePrice * 0.98m;
        }

        public string GetPaymentMethod() => "Card";
    }
}


