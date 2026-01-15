namespace TableConverter.Formatters;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Formatter that generates JSON output in array format.
/// </summary>
public class JsonFormatter : ITableFormatter
{
    public bool RequiresFileOutput => false;

    /// <summary>
    /// Formats table data as JSON array.
    /// </summary>
    public void Format(IEnumerable<IDictionary<string, object>> data, TextWriter output)
    {
        var dataList = data.ToList();

        // Convert to JSON-friendly format
        var jsonData = dataList.Select(row => ConvertRowForJson(row)).ToList();

        var options = new JsonSerializerOptions
        {
            WriteIndented = true, // Pretty print
            DefaultIgnoreCondition = JsonIgnoreCondition.Never, // Include nulls
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonDecimalConverter() } // Preserve decimal precision
        };

        var json = JsonSerializer.Serialize(jsonData, options);
        output.WriteLine(json);
    }

    private static Dictionary<string, object?> ConvertRowForJson(IDictionary<string, object> row)
    {
        var result = new Dictionary<string, object?>();

        foreach (var kvp in row)
        {
            result[kvp.Key] = ConvertValueForJson(kvp.Value);
        }

        return result;
    }

    private static object? ConvertValueForJson(object? value)
    {
        // NULL/DBNull -> JSON null
        if (value is null or DBNull)
        {
            return null;
        }

        // DateTime - ISO 8601 string
        if (value is DateTime dateValue)
        {
            return dateValue.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        // DateTimeOffset
        if (value is DateTimeOffset dateTimeOffsetValue)
        {
            return dateTimeOffsetValue.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture);
        }

        // TimeSpan - string format
        if (value is TimeSpan timeSpanValue)
        {
            return timeSpanValue.ToString(@"hh\:mm\:ss\.fffffff", CultureInfo.InvariantCulture);
        }

        // Boolean - JSON boolean
        if (value is bool boolValue)
        {
            return boolValue;
        }

        // Guid - string format
        if (value is Guid guidValue)
        {
            return guidValue.ToString("D");
        }

        // Byte array - hex string
        if (value is byte[] byteArray)
        {
            return byteArray.Length == 0 ? "" : BitConverter.ToString(byteArray).Replace("-", "");
        }

        // Numeric types - preserve as numbers
        if (value is decimal or double or float or int or long or short or byte or sbyte or uint or ulong or ushort)
        {
            return value;
        }

        // String and other types
        return value;
    }
}

/// <summary>
/// Custom JSON converter for decimal to preserve precision.
/// </summary>
public class JsonDecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDecimal();
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        // Write as string to preserve full precision
        writer.WriteStringValue(value.ToString("G29", CultureInfo.InvariantCulture));
    }
}
