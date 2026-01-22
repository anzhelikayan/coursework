using System;
using Newtonsoft.Json;

namespace BusStationTicketSystem.Patterns.Repository
{
    public class TimeSpanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return objectType == typeof(TimeSpan?) ? null : (object)TimeSpan.Zero;

            if (reader.Value is string timeString)
            {
                // Спробуємо різні формати часу
                if (TimeSpan.TryParse(timeString, out TimeSpan result))
                    return result;
                
                // Спробуємо формат HH:mm:ss
                if (TimeSpan.TryParseExact(timeString, @"hh\:mm\:ss", null, out result))
                    return result;
                
                // Спробуємо формат HH:mm
                if (TimeSpan.TryParseExact(timeString, @"hh\:mm", null, out result))
                    return result;
            }

            return objectType == typeof(TimeSpan?) ? null : (object)TimeSpan.Zero;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else if (value is TimeSpan timeSpan)
            {
                writer.WriteValue(timeSpan.ToString(@"hh\:mm\:ss"));
            }
        }
    }
}

