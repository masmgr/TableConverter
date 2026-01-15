namespace TableConverter.Models;

/// <summary>
/// Supported output formats for table conversion.
/// </summary>
public enum OutputFormat
{
    /// <summary>SQL INSERT statements with Table Values Constructor format.</summary>
    Sql = 0,

    /// <summary>RFC 4180 compliant CSV format.</summary>
    Csv = 1,

    /// <summary>JSON array format with objects.</summary>
    Json = 2,

    /// <summary>Excel .xlsx format.</summary>
    Excel = 3
}
