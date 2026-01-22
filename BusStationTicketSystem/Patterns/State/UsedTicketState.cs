using System;

namespace BusStationTicketSystem.Patterns.State
{
    public class UsedTicketState : ITicketState
    {
        public void Activate()
        {
            throw new InvalidOperationException("Не можна активувати використаний квиток");
        }

        public void Cancel()
        {
            throw new InvalidOperationException("Не можна скасувати використаний квиток");
        }

        public void Use()
        {
        }

        public string GetStatus() => "Used";
    }
}


