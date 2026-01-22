using System;
using BusStationTicketSystem.Models;

namespace BusStationTicketSystem.Patterns.Builder
{
    public class TicketBuilder
    {
        private Ticket _ticket = new Ticket();

        public TicketBuilder SetRoute(Route route)
        {
            _ticket.Route = route;
            return this;
        }

        public TicketBuilder SetPassenger(Passenger passenger)
        {
            _ticket.Passenger = passenger;
            return this;
        }

        public TicketBuilder SetDate(DateTime date)
        {
            _ticket.Date = date;
            return this;
        }

        public TicketBuilder SetPrice(decimal price)
        {
            _ticket.Price = price;
            return this;
        }

        public TicketBuilder SetTicketType(string ticketType)
        {
            _ticket.TicketType = ticketType;
            return this;
        }

        public TicketBuilder SetPaymentMethod(string paymentMethod)
        {
            _ticket.PaymentMethod = paymentMethod;
            return this;
        }

        public TicketBuilder SetStatus(string status)
        {
            _ticket.Status = status;
            return this;
        }

        public TicketBuilder SetId(int id)
        {
            _ticket.Id = id;
            return this;
        }

        public Ticket Build()
        {
            return _ticket;
        }

        public void Reset()
        {
            _ticket = new Ticket();
        }
    }
}

