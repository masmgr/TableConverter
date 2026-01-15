namespace TableConverter.Models;

/// <summary>
/// Represents parsed command-line options for TableConverter.
/// </summary>
public class CommandLineOptions
{
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public OutputFormat Format { get; set; } = OutputFormat.Sql;
    public string? OutputPath { get; set; } = null;

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Server) &&
        !string.IsNullOrWhiteSpace(Database) &&
        !string.IsNullOrWhiteSpace(Query);

    /// <summary>
    /// Parses command-line arguments into CommandLineOptions.
    /// Supports both backward-compatible 3-argument form and new named argument form.
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <returns>Parsed options</returns>
    /// <exception cref="ArgumentException">If format is invalid</exception>
    public static CommandLineOptions Parse(string[] args)
    {
        var options = new CommandLineOptions();

        // Backward compatibility: 3 positional args = SQL format to console
        if (args.Length == 3 && !args.Any(a => a.StartsWith("--")))
        {
            options.Server = args[0];
            options.Database = args[1];
            options.Query = args[2];
            options.Format = OutputFormat.Sql;
            options.OutputPath = null;
            return options;
        }

        // Parse named arguments
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLowerInvariant())
            {
                case "--server":
                case "-s":
                    if (i + 1 < args.Length) options.Server = args[++i];
                    break;

                case "--database":
                case "-d":
                    if (i + 1 < args.Length) options.Database = args[++i];
                    break;

                case "--query":
                case "-q":
                    if (i + 1 < args.Length) options.Query = args[++i];
                    break;

                case "--format":
                case "-f":
                    if (i + 1 < args.Length)
                    {
                        var formatStr = args[++i];
                        if (Enum.TryParse<OutputFormat>(formatStr, true, out var format))
                        {
                            options.Format = format;
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid format: {formatStr}. Valid formats: SQL, CSV, JSON, Excel");
                        }
                    }
                    break;

                case "--output":
                case "-o":
                    if (i + 1 < args.Length) options.OutputPath = args[++i];
                    break;

                default:
                    // First three non-option arguments are server, database, query (backward compat)
                    if (!args[i].StartsWith("--") && !args[i].StartsWith("-"))
                    {
                        if (string.IsNullOrEmpty(options.Server))
                            options.Server = args[i];
                        else if (string.IsNullOrEmpty(options.Database))
                            options.Database = args[i];
                        else if (string.IsNullOrEmpty(options.Query))
                            options.Query = args[i];
                    }
                    break;
            }
        }

        return options;
    }
}
