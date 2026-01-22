namespace BusStationTicketSystem.Patterns.Strategy
{
    public interface IPaymentStrategy
    {
        decimal CalculatePrice(decimal basePrice);
        string GetPaymentMethod();
    }
}


