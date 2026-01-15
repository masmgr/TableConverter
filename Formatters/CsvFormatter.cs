namespace TableConverter.Formatters;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

/// <summary>
/// Formatter that generates RFC 4180 compliant CSV output.
/// </summary>
public class CsvFormatter : ITableFormatter
{
    public bool RequiresFileOutput => false;

    /// <summary>
    /// Formats table data as CSV.
    /// </summary>
    public void Format(IEnumerable<IDictionary<string, object>> data, TextWriter output)
    {
        var dataList = data.ToList();

        if (dataList.Count == 0)
        {
            // Empty CSV - no output
            return;
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            // Escape double quotes by doubling them (RFC 4180)
            Escape = '"',
            Quote = '"',
            // Always quote strings to handle special characters
            ShouldQuote = args => args.Field is string
        };

        using var csv = new CsvWriter(output, config);

        // Write headers
        var firstRow = dataList.First();
        foreach (var key in firstRow.Keys)
        {
            csv.WriteField(key);
        }
        csv.NextRecord();

        // Write data rows
        foreach (var row in dataList)
        {
            foreach (var value in row.Values)
            {
                csv.WriteField(ConvertForCsv(value));
            }
            csv.NextRecord();
        }

        csv.Flush();
    }

    private static object? ConvertForCsv(object? value)
    {
        // Handle NULL/DBNull
        if (value is null or DBNull)
        {
            return null; // CsvHelper will output empty field
        }

        // DateTime - use ISO 8601 format
        if (value is DateTime dateValue)
        {
            return dateValue.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        // DateTimeOffset
        if (value is DateTimeOffset dateTimeOffsetValue)
        {
            return dateTimeOffsetValue.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture);
        }

        // TimeSpan
        if (value is TimeSpan timeSpanValue)
        {
            return timeSpanValue.ToString(@"hh\:mm\:ss\.fffffff", CultureInfo.InvariantCulture);
        }

        // Boolean - output as 1/0 for consistency with SQL
        if (value is bool boolValue)
        {
            return boolValue ? "1" : "0";
        }

        // Guid
        if (value is Guid guidValue)
        {
            return guidValue.ToString("D");
        }

        // Byte array - hex string
        if (value is byte[] byteArray)
        {
            return byteArray.Length == 0 ? "" : BitConverter.ToString(byteArray).Replace("-", "");
        }

        // Numeric types - use invariant culture
        if (value is decimal or double or float)
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        // Default: use invariant culture conversion
        return value;
    }
}
