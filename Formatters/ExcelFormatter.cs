namespace TableConverter.Formatters;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ClosedXML.Excel;

/// <summary>
/// Formatter that generates Excel .xlsx files.
/// </summary>
public class ExcelFormatter : ITableFormatter
{
    private readonly string _filePath;

    public bool RequiresFileOutput => true;

    /// <summary>
    /// Creates a new ExcelFormatter.
    /// </summary>
    /// <param name="filePath">Path where the Excel file will be saved.</param>
    public ExcelFormatter(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    /// <summary>
    /// Formats table data as an Excel workbook.
    /// </summary>
    public void Format(IEnumerable<IDictionary<string, object>> data, TextWriter output)
    {
        var dataList = data.ToList();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Data");

        if (dataList.Count == 0)
        {
            // Empty workbook with no data
            workbook.SaveAs(_filePath);
            output.WriteLine($"Excel file created (empty): {_filePath}");
            return;
        }

        // Write headers in row 1
        var firstRow = dataList.First();
        int col = 1;
        foreach (var key in firstRow.Keys)
        {
            worksheet.Cell(1, col).Value = key;
            worksheet.Cell(1, col).Style.Font.Bold = true;
            col++;
        }

        // Write data rows starting from row 2
        int row = 2;
        foreach (var dataRow in dataList)
        {
            col = 1;
            foreach (var value in dataRow.Values)
            {
                SetCellValue(worksheet.Cell(row, col), value);
                col++;
            }
            row++;
        }

        // Auto-fit columns for better readability
        worksheet.Columns().AdjustToContents();

        // Save workbook
        workbook.SaveAs(_filePath);
        output.WriteLine($"Excel file created: {_filePath}");
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        // NULL/DBNull -> empty cell
        if (value is null or DBNull)
        {
            cell.Value = "";
            return;
        }

        // DateTime -> Excel date
        if (value is DateTime dateValue)
        {
            cell.Value = dateValue;
            cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss.000";
            return;
        }

        // DateTimeOffset -> Excel date (convert to local)
        if (value is DateTimeOffset dateTimeOffsetValue)
        {
            cell.Value = dateTimeOffsetValue.DateTime;
            cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss.000 zzz";
            return;
        }

        // TimeSpan -> formatted string
        if (value is TimeSpan timeSpanValue)
        {
            cell.Value = timeSpanValue.ToString(@"hh\:mm\:ss\.fffffff", CultureInfo.InvariantCulture);
            return;
        }

        // Boolean -> 1/0 numeric for consistency
        if (value is bool boolValue)
        {
            cell.Value = boolValue ? 1 : 0;
            return;
        }

        // Guid -> string
        if (value is Guid guidValue)
        {
            cell.Value = guidValue.ToString("D");
            return;
        }

        // Byte array -> hex string
        if (value is byte[] byteArray)
        {
            cell.Value = byteArray.Length == 0 ? "" : BitConverter.ToString(byteArray).Replace("-", "");
            return;
        }

        // Numeric types -> preserve as numbers
        if (value is decimal decimalValue)
        {
            cell.Value = decimalValue;
            cell.Style.NumberFormat.Format = "0.00000000000000000000000000000"; // 29 decimal places
            return;
        }

        if (value is double doubleValue)
        {
            cell.Value = doubleValue;
            return;
        }

        if (value is float floatValue)
        {
            cell.Value = floatValue;
            return;
        }

        if (value is int or long or short or byte or sbyte or uint or ulong or ushort)
        {
            cell.Value = Convert.ToInt64(value);
            return;
        }

        // String and default
        cell.Value = value.ToString() ?? "";
    }
}
