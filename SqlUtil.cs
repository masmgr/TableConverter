namespace TableConverter;

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
    /// </summary>
    public static string GetSqlValue(object value)
    {
        if (value is null)
        {
            return "null";
        }
        else if (value is string strValue)
        {
            return $"N'{EscapeSqlString(strValue)}'";
        }
        else if (value is DateTime dateValue)
        {
            return $"N'{dateValue.ToString()}'";
        }
        else
        {
            return value?.ToString() ?? "null";
        }
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