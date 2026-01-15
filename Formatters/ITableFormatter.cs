namespace TableConverter.Formatters;

/// <summary>
/// Interface for table data formatters.
/// </summary>
public interface ITableFormatter
{
    /// <summary>
    /// Formats table data to the target format.
    /// </summary>
    /// <param name="data">Query result data as enumerable of dictionaries.</param>
    /// <param name="output">TextWriter for output (Console.Out or StreamWriter).</param>
    void Format(IEnumerable<IDictionary<string, object>> data, TextWriter output);

    /// <summary>
    /// Indicates if this formatter requires file output (like Excel).
    /// </summary>
    bool RequiresFileOutput { get; }
}
