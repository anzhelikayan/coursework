using System;
using BusStationTicketSystem.Models;

namespace BusStationTicketSystem.Patterns.TemplateMethod
{
    public abstract class TicketProcessor
    {
        public void ProcessTicket(Ticket ticket)
        {
            ValidateTicket(ticket);
            CalculatePrice(ticket);
            ApplyDiscounts(ticket);
            FinalizeTicket(ticket);
        }

        protected virtual void ValidateTicket(Ticket ticket)
        {
            if (ticket.Route == null)
                throw new ArgumentException("Маршрут не вказано");
            if (ticket.Passenger == null)
                throw new ArgumentException("Пасажир не вказано");
            if (string.IsNullOrWhiteSpace(ticket.Passenger.LastName) || string.IsNullOrWhiteSpace(ticket.Passenger.FirstName))
                throw new ArgumentException("Ім'я пасажира не вказано");
            if (ticket.Date < DateTime.Now.Date)
                throw new ArgumentException("Дата поїздки не може бути в минулому");
        }

        protected abstract void CalculatePrice(Ticket ticket);
        
        protected virtual void ApplyDiscounts(Ticket ticket)
        {
        }

        protected virtual void FinalizeTicket(Ticket ticket)
        {
            ticket.Status = "Active";
        }
    }
}


