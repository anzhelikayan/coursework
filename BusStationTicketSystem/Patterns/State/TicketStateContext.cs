using System;

namespace BusStationTicketSystem.Patterns.State
{
    public class TicketStateContext
    {
        private ITicketState _state;

        public TicketStateContext()
        {
            _state = new ActiveTicketState();
        }

        public void SetState(ITicketState state)
        {
            _state = state;
        }

        public void Activate()
        {
            _state.Activate();
        }

        public void Cancel()
        {
            if (_state is UsedTicketState)
            {
                throw new InvalidOperationException("Не можна скасувати використаний квиток");
            }
            _state.Cancel();
            _state = new CancelledTicketState();
        }

        public void Use()
        {
            if (_state is CancelledTicketState)
            {
                throw new InvalidOperationException("Не можна використати скасований квиток");
            }
            _state.Use();
            _state = new UsedTicketState();
        }

        public string GetStatus()
        {
            return _state.GetStatus();
        }
    }
}

