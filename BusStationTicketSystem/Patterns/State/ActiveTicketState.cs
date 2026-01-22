using System;

namespace BusStationTicketSystem.Patterns.State
{
    public class ActiveTicketState : ITicketState
    {
        public void Activate()
        {
        }

        public void Cancel()
        {
        }

        public void Use()
        {
        }

        public string GetStatus() => "Active";
    }
}

