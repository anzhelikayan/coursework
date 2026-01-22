using System;
using Newtonsoft.Json;

namespace BusStationTicketSystem.Patterns.Repository
{
    public class DateTimeConverter : JsonConverter
    {
        private const string DateFormat = "yyyy-MM-ddTHH:mm:ss";
        private const string DateOnlyFormat = "yyyy-MM-dd";

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return objectType == typeof(DateTime?) ? null : (object)DateTime.MinValue;

            if (reader.Value is string dateString)
            {
                // Спробуємо різні формати
                if (DateTime.TryParseExact(dateString, DateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime result))
                    return result;
                
                if (DateTime.TryParseExact(dateString, DateOnlyFormat, null, System.Globalization.DateTimeStyles.None, out result))
                    return result;
                
                // Якщо не вдалося, спробуємо стандартний парсинг
                if (DateTime.TryParse(dateString, out result))
                    return result;
            }
            else if (reader.Value is DateTime dateTime)
            {
                return dateTime;
            }

            return objectType == typeof(DateTime?) ? null : (object)DateTime.MinValue;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else if (value is DateTime dateTime)
            {
                writer.WriteValue(dateTime.ToString(DateFormat));
            }
        }
    }
}

