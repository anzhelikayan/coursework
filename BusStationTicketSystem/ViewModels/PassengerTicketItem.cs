using System;
using System.ComponentModel;

namespace BusStationTicketSystem.ViewModels
{
    public class PassengerTicketItem : INotifyPropertyChanged
    {
        private int _selectedTicketTypeIndex = 0;
        private string _passengerLastName = string.Empty;
        private string _passengerFirstName = string.Empty;
        private string _passengerMiddleName = string.Empty;
        private string _passengerPhone = string.Empty;
        private string _passengerDocument = string.Empty;

        public int TicketNumber { get; set; }

        public int SelectedTicketTypeIndex
        {
            get => _selectedTicketTypeIndex;
            set
            {
                _selectedTicketTypeIndex = value;
                OnPropertyChanged(nameof(SelectedTicketTypeIndex));
            }
        }

        public string PassengerLastName
        {
            get => _passengerLastName;
            set
            {
                _passengerLastName = value;
                OnPropertyChanged(nameof(PassengerLastName));
            }
        }

        public string PassengerFirstName
        {
            get => _passengerFirstName;
            set
            {
                _passengerFirstName = value;
                OnPropertyChanged(nameof(PassengerFirstName));
            }
        }

        public string PassengerMiddleName
        {
            get => _passengerMiddleName;
            set
            {
                _passengerMiddleName = value;
                OnPropertyChanged(nameof(PassengerMiddleName));
            }
        }

        // Властивість для сумісності (повне ім'я)
        public string PassengerName
        {
            get => $"{PassengerLastName} {PassengerFirstName} {PassengerMiddleName}".Trim();
            set
            {
                // Розбиваємо повне ім'я на складові, якщо передано
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    PassengerLastName = parts.Length > 0 ? parts[0] : string.Empty;
                    PassengerFirstName = parts.Length > 1 ? parts[1] : string.Empty;
                    PassengerMiddleName = parts.Length > 2 ? parts[2] : string.Empty;
                }
            }
        }

        public string PassengerPhone
        {
            get => _passengerPhone;
            set
            {
                _passengerPhone = value;
                OnPropertyChanged(nameof(PassengerPhone));
            }
        }

        public string PassengerDocument
        {
            get => _passengerDocument;
            set
            {
                _passengerDocument = value;
                OnPropertyChanged(nameof(PassengerDocument));
            }
        }

        public string GetTicketType()
        {
            return SelectedTicketTypeIndex switch
            {
                0 => "Regular",
                1 => "Discount",
                2 => "Child",
                _ => "Regular"
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

