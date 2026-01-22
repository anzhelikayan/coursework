using System;
using BusStationTicketSystem.Models;

namespace BusStationTicketSystem.Patterns.TemplateMethod
{
    public class StandardTicketProcessor : TicketProcessor
    {
        protected override void CalculatePrice(Ticket ticket)
        {
            if (ticket.Price <= 0)
            {
                throw new ArgumentException("Ціна квитка повинна бути більше нуля");
            }

            if (ticket.Route != null && ticket.Price > ticket.Route.BasePrice * 2)
            {
                throw new ArgumentException("Ціна квитка перевищує допустимий ліміт");
            }
        }

        protected override void ApplyDiscounts(Ticket ticket)
        {
            var daysUntilDeparture = (ticket.Date.Date - DateTime.Now.Date).Days;
            
            if (daysUntilDeparture >= 30 && ticket.TicketType == "Regular")
            {
                ticket.Price *= 0.95m;
            }
            
            base.ApplyDiscounts(ticket);
        }

        protected override void FinalizeTicket(Ticket ticket)
        {
            base.FinalizeTicket(ticket);
            
            if (ticket.Id == 0)
            {
                ticket.Id = DateTime.Now.GetHashCode() & 0x7FFFFFFF;
            }
        }
    }
}

