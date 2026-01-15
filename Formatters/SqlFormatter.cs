namespace TableConverter.Formatters;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Formatter that generates SQL INSERT statements using Table Values Constructor format.
/// </summary>
public class SqlFormatter : ITableFormatter
{
    private const int MAX_ROWS = 500;

    public bool RequiresFileOutput => false;

    /// <summary>
    /// Formats table data as SQL INSERT statements.
    /// Splits results into chunks of MAX_ROWS rows per INSERT statement.
    /// </summary>
    public void Format(IEnumerable<IDictionary<string, object>> data, TextWriter output)
    {
        var dataList = data.ToList();

        if (dataList.Count == 0)
        {
            output.WriteLine("-- No rows returned from query");
            return;
        }

        var columnNames = SqlUtil.GetColumnNames(dataList.First()).ToList();
        var valuesRows = dataList
            .Select(row => SqlUtil.GetSqlValueRow(row.Select(item => SqlUtil.GetSqlValue(item.Value))));

        var valuesRowsChunks = valuesRows.Chunk(MAX_ROWS);

        foreach (var valuesRowsChunk in valuesRowsChunks)
        {
            output.WriteLine($"INSERT INTO @source_table ({string.Join(", ", columnNames)})");
            output.WriteLine($"VALUES");
            var separator = $",{Environment.NewLine}";
            output.WriteLine($"{string.Join(separator, valuesRowsChunk)};");
        }
    }
}
