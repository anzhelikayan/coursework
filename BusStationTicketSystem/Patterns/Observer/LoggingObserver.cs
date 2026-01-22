using System;
using System.IO;
using System.Text;
using BusStationTicketSystem.Models;

namespace BusStationTicketSystem.Patterns.Observer
{
    public class LoggingObserver : ITicketObserver
    {
        private const string LogFileName = "ticket_system.log";
        private readonly object _lockObject = new object();

        private void WriteLog(string message)
        {
            lock (_lockObject)
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                File.AppendAllText(LogFileName, logEntry, Encoding.UTF8);
            }
        }

        public void OnTicketCreated(Ticket ticket)
        {
            WriteLog($"Створено квиток #{ticket.Id} для {ticket.PassengerName}");
        }

        public void OnTicketCancelled(Ticket ticket)
        {
            WriteLog($"Скасовано квиток #{ticket.Id} для {ticket.PassengerName}");
        }

        public void OnTicketStatusChanged(Ticket ticket, string oldStatus, string newStatus)
        {
            WriteLog($"Змінено статус квитка #{ticket.Id} з {oldStatus} на {newStatus}");
        }
    }
}
