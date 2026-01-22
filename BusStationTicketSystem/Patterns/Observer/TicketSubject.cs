using System.Collections.Generic;
using BusStationTicketSystem.Models;

namespace BusStationTicketSystem.Patterns.Observer
{
    public class TicketSubject
    {
        private readonly List<ITicketObserver> _observers = new List<ITicketObserver>();

        public void Attach(ITicketObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(ITicketObserver observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyTicketCreated(Ticket ticket)
        {
            foreach (var observer in _observers)
            {
                observer.OnTicketCreated(ticket);
            }
        }

        public void NotifyTicketCancelled(Ticket ticket)
        {
            foreach (var observer in _observers)
            {
                observer.OnTicketCancelled(ticket);
            }
        }

        public void NotifyTicketStatusChanged(Ticket ticket, string oldStatus, string newStatus)
        {
            foreach (var observer in _observers)
            {
                observer.OnTicketStatusChanged(ticket, oldStatus, newStatus);
            }
        }
    }
}


