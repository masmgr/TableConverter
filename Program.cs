namespace TableConverter;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Dapper;
using TableConverter.Formatters;
using TableConverter.Models;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            ShowUsage();
            Environment.Exit(1);
            return;
        }

        try
        {
            // Parse command-line arguments
            var options = CommandLineOptions.Parse(args);

            if (!options.IsValid)
            {
                ShowUsage();
                Environment.Exit(1);
                return;
            }

            // Build connection string
            var connectionString = new SqlConnectionStringBuilder()
            {
                DataSource = options.Server,
                InitialCatalog = options.Database,
                IntegratedSecurity = true
            }.ToString();

            // Query database
            var data = QueryDatabase(connectionString, options.Query);

            // Get formatter
            var formatter = CreateFormatter(options);

            // Validate output requirements
            if (formatter.RequiresFileOutput && string.IsNullOrWhiteSpace(options.OutputPath))
            {
                Console.Error.WriteLine($"Error: {options.Format} format requires --output parameter with file path");
                Environment.Exit(4);
                return;
            }

            // Format and output
            if (string.IsNullOrWhiteSpace(options.OutputPath))
            {
                // Output to console
                formatter.Format(data, Console.Out);
            }
            else
            {
                // Output to file
                using var writer = new StreamWriter(options.OutputPath, false, System.Text.Encoding.UTF8);
                formatter.Format(data, writer);
            }
        }
        catch (SqlException ex)
        {
            Console.Error.WriteLine($"Database error: {ex.Message}");
            Environment.Exit(2);
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"Query returned no results: {ex.Message}");
            Environment.Exit(3);
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"File I/O error: {ex.Message}");
            Environment.Exit(5);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine($"File access denied: {ex.Message}");
            Environment.Exit(6);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"Invalid argument: {ex.Message}");
            Environment.Exit(7);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            Environment.Exit(99);
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("TableConverter - Convert SQL Server query results to various formats");
        Console.WriteLine();
        Console.WriteLine("Usage (backward compatible):");
        Console.WriteLine("  TableConverter <server> <database> <query>");
        Console.WriteLine();
        Console.WriteLine("Usage (with options):");
        Console.WriteLine("  TableConverter <server> <database> <query> [--format <format>] [--output <file>]");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <server>          SQL Server instance name (e.g., localhost, .\\SQLEXPRESS)");
        Console.WriteLine("  <database>        Database name");
        Console.WriteLine("  <query>           SELECT query to execute");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --format, -f      Output format: SQL (default), CSV, JSON, Excel");
        Console.WriteLine("  --output, -o      Output file path (if not specified, writes to console)");
        Console.WriteLine("                    Note: Excel format requires --output parameter");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  TableConverter .\\SQLEXPRESS MyDB \"SELECT * FROM Users\"");
        Console.WriteLine("  TableConverter .\\SQLEXPRESS MyDB \"SELECT * FROM Users\" --format CSV");
        Console.WriteLine("  TableConverter .\\SQLEXPRESS MyDB \"SELECT * FROM Users\" --format JSON --output users.json");
        Console.WriteLine("  TableConverter .\\SQLEXPRESS MyDB \"SELECT * FROM Users\" --format Excel --output users.xlsx");
    }

    private static List<IDictionary<string, object>> QueryDatabase(string connectionString, string query)
    {
        using var conn = new SqlConnection(connectionString);
        conn.Open();
        var results = conn.Query(query).ToList();
        return results.OfType<IDictionary<string, object>>().ToList();
    }

    private static ITableFormatter CreateFormatter(CommandLineOptions options)
    {
        return options.Format switch
        {
            OutputFormat.Sql => new SqlFormatter(),
            OutputFormat.Csv => new CsvFormatter(),
            OutputFormat.Json => new JsonFormatter(),
            OutputFormat.Excel => new ExcelFormatter(options.OutputPath ?? throw new InvalidOperationException("Excel format requires output path")),
            _ => throw new NotSupportedException($"Format {options.Format} is not supported")
        };
    }

    /// <summary>
    /// [DEPRECATED] Generates SQL INSERT statements using VALUES constructor from query results.
    /// This method is kept for backward compatibility with tests.
    /// Use the new formatter architecture instead.
    /// </summary>
    [Obsolete("Use SqlFormatter instead")]
    public static string GenerateTableValuesConstructor(string connectionString, string query)
    {
        var data = QueryDatabase(connectionString, query);
        var formatter = new SqlFormatter();
        var output = new StringWriter();
        formatter.Format(data, output);
        return output.ToString();
    }
}
