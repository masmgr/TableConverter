namespace TableConverter;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Dapper;

class Program
{
    const int MAX_ROWS = 500;

    static void Main(string[] args)
    {
        string connectionString = new SqlConnectionStringBuilder()
        {
            DataSource = args[0],
            InitialCatalog = args[1],
            IntegratedSecurity = true
        }.ToString();

        generateTableValuesConstructor(connectionString, args[2]);
    }

    public static string generateTableValuesConstructor(string connectionString, string query)
    {
        var queryBuilder = new StringBuilder();

        using (var conn = new SqlConnection(connectionString))
        {
            conn.Open();

            var results = conn.Query(query);

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
