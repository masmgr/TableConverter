namespace TableConverter;

using System.Globalization;

/// <summary>
/// Utility class for SQL.
/// </summary>
public static class SqlUtil
{
    /// <summary>
    /// Escapes a string for SQL.
    /// </summary>
    public static string EscapeSqlString(string str) => str.Replace("'", "''");

    /// <summary>
    /// Gets the SQL value for an object.
    /// Converts .NET values to SQL-compatible string representations.
    /// All conversions use CultureInfo.InvariantCulture for consistency.
    /// </summary>
    public static string GetSqlValue(object value)
    {
        // null and DBNull handling
        if (value is null or DBNull)
        {
            return "NULL";
        }

        // String type
        if (value is string strValue)
        {
            return $"N'{EscapeSqlString(strValue)}'";
        }

        // DateTime type - ISO 8601 format (SQL Server compatible)
        if (value is DateTime dateValue)
        {
            // 'yyyy-MM-ddTHH:mm:ss.fff' format
            return $"'{dateValue.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture)}'";
        }

        // DateTimeOffset type
        if (value is DateTimeOffset dateTimeOffsetValue)
        {
            return $"'{dateTimeOffsetValue.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture)}'";
        }

        // TimeSpan type
        if (value is TimeSpan timeSpanValue)
        {
            return $"'{timeSpanValue.ToString(@"hh\:mm\:ss\.fffffff", CultureInfo.InvariantCulture)}'";
        }

        // Boolean type
        if (value is bool boolValue)
        {
            return boolValue ? "1" : "0";
        }

        // Guid type
        if (value is Guid guidValue)
        {
            return $"'{guidValue:D}'";
        }

        // Decimal type - preserve precision
        if (value is decimal decimalValue)
        {
            // G29 format preserves up to 29 significant digits
            return decimalValue.ToString("G29", CultureInfo.InvariantCulture);
        }

        // Double type - preserve precision
        if (value is double doubleValue)
        {
            // Check for special values
            if (double.IsNaN(doubleValue)) return "NULL";
            if (double.IsPositiveInfinity(doubleValue)) return "NULL";
            if (double.IsNegativeInfinity(doubleValue)) return "NULL";

            // G17 format guarantees complete round-trip
            return doubleValue.ToString("G17", CultureInfo.InvariantCulture);
        }

        // Float type - preserve precision
        if (value is float floatValue)
        {
            // Check for special values
            if (float.IsNaN(floatValue)) return "NULL";
            if (float.IsPositiveInfinity(floatValue)) return "NULL";
            if (float.IsNegativeInfinity(floatValue)) return "NULL";

            // G9 format guarantees complete round-trip
            return floatValue.ToString("G9", CultureInfo.InvariantCulture);
        }

        // Integer types (sbyte, byte, short, ushort, int, uint, long, ulong)
        if (value is sbyte or byte or short or ushort or int or uint or long or ulong)
        {
            return value.ToString();
        }

        // Byte array (binary data)
        if (value is byte[] byteArray)
        {
            if (byteArray.Length == 0)
            {
                return "0x";
            }
            // Convert to hexadecimal string format
            return $"0x{BitConverter.ToString(byteArray).Replace("-", "")}";
        }

        // Fallback for other types
        var result = Convert.ToString(value, CultureInfo.InvariantCulture);
        return result ?? "NULL";
    }

    /// <summary>
    /// Gets the SQL value row for a row of values.
    /// </summary>
    public static string GetSqlValueRow(IEnumerable<string> row)
    {
        var separator = ", ";
        return $"({string.Join(separator, row)})";
    }

    /// <summary>
    /// Gets the SQL column name for a column name.
    /// </summary>
    public static string GetColumnName(string columnName) => $"[{columnName}]";

    /// <summary>
    /// Gets the SQL column names for a row.
    /// </summary>
    public static IEnumerable<string> GetColumnNames(IDictionary<string, object> row)
    {
        return row.Keys.Select(GetColumnName);
    }
}