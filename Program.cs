namespace TableConverter;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Dapper;

class Program
{
    const int MAX_ROWS = 500;

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
            var connectionString = new SqlConnectionStringBuilder()
            {
                DataSource = args[0],
                InitialCatalog = args[1],
                IntegratedSecurity = true
            }.ToString();

            var result = GenerateTableValuesConstructor(connectionString, args[2]);
            Console.WriteLine(result);
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
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            Environment.Exit(99);
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("TableConverter - Generate SQL INSERT statements from query results");
        Console.WriteLine();
        Console.WriteLine("Usage: TableConverter <server> <database> <query>");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <server>    SQL Server instance name (e.g., localhost, .\\SQLEXPRESS)");
        Console.WriteLine("  <database>  Database name");
        Console.WriteLine("  <query>     SELECT query to execute");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine("  TableConverter .\\SQLEXPRESS MyDB \"SELECT * FROM Users WHERE Active = 1\"");
    }

    /// <summary>
    /// Generates SQL INSERT statements using VALUES constructor from query results.
    /// Splits results into chunks of MAX_ROWS rows per INSERT statement.
    /// </summary>
    /// <param name="connectionString">SQL Server connection string.</param>
    /// <param name="query">SELECT query to execute.</param>
    /// <returns>Generated SQL INSERT statements. Returns a comment if the query returns no results.</returns>
    /// <exception cref="SqlException">Thrown when database connection or query execution fails.</exception>
    public static string GenerateTableValuesConstructor(string connectionString, string query)
    {
        var queryBuilder = new StringBuilder();

        using (var conn = new SqlConnection(connectionString))
        {
            conn.Open();

            var results = conn.Query(query).ToList();

            // Check for empty result set
            if (results.Count == 0)
            {
                return "-- No rows returned from query";
            }

            var columnNames = SqlUtil.GetColumnNames(results.First()).ToList();
            var valuesDict = results
                .OfType<IDictionary<string, object>>();

            var valuesRows = valuesDict
                .Select(row => SqlUtil.GetSqlValueRow(row.Select(item => SqlUtil.GetSqlValue(item.Value))));

            var valuesRowsChunks = valuesRows.Chunk(MAX_ROWS);

            foreach (var valuesRowsChunk in valuesRowsChunks)
            {
                queryBuilder.AppendLine($"INSERT INTO @source_table ({string.Join(", ", columnNames)})");
                queryBuilder.AppendLine($"VALUES");
                var separator = $",{Environment.NewLine}";
                queryBuilder.AppendLine($"{string.Join(separator, valuesRowsChunk)};");
            }
        }

        return queryBuilder.ToString();
    }
}
