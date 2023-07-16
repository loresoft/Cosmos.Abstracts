using System;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cosmos.Abstracts.Converters;

/// <summary>
/// Unix time converter.
/// </summary>
public class UnixTimeConverter : DateTimeConverterBase
{
    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
        }
        else if (value is DateTimeOffset dateTimeOffset)
        {
            var seconds = dateTimeOffset.ToUnixTimeSeconds();

            writer.WriteValue(seconds);
        }
        else if (value is DateTime dateTime)
        {
            var offset = new DateTimeOffset(dateTime);
            var seconds = offset.ToUnixTimeSeconds();

            writer.WriteValue(seconds);
        }
        else
        {
            throw new ArgumentException("Invalid datetime value", nameof(value));
        }
    }

    /// <inheritdoc/>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        if (reader.TokenType != JsonToken.Integer)
            throw new InvalidOperationException("Invalid integer token");

        var seconds = Convert.ToInt64(reader.Value, CultureInfo.InvariantCulture);
        var offset = DateTimeOffset.FromUnixTimeSeconds(seconds);

        if (objectType == typeof(DateTime))
            return offset.DateTime;

        return offset;
    }
}
