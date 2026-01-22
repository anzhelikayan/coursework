using System;

namespace BusStationTicketSystem.Patterns.State
{
    public class CancelledTicketState : ITicketState
    {
        public void Activate()
        {
            throw new InvalidOperationException("Не можна активувати скасований квиток");
        }

        public void Cancel()
        {
        }

        public void Use()
        {
            throw new InvalidOperationException("Не можна використати скасований квиток");
        }

        public string GetStatus() => "Cancelled";
    }
}


