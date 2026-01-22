namespace BusStationTicketSystem.Patterns.State
{
    public interface ITicketState
    {
        void Activate();
        void Cancel();
        void Use();
        string GetStatus();
    }
}

