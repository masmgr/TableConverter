# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Test Commands

### Building the project
```bash
dotnet build
```
Build in Release configuration (as used in CI):
```bash
dotnet build --configuration Release
```

### Running tests
```bash
dotnet test
```
Run specific test class:
```bash
dotnet test --filter "ClassName"
```
Run specific test method:
```bash
dotnet test --filter "ClassName.MethodName"
```

### Watching during development
```bash
dotnet watch run --project TableConverter.csproj
```

### Publishing a release build
```bash
dotnet publish
```

## Project Architecture

This is a .NET 8.0 console application that converts SQL Server query results into various formats.

### Core Components

**[Program.cs](Program.cs)** - Entry point and orchestration
- Parses command-line arguments via `CommandLineOptions.Parse()`
- Builds SQL Server connection string (uses Windows authentication)
- Queries the database and retrieves results as `List<IDictionary<string, object>>`
- Creates appropriate formatter based on output format
- Handles all exception types and maps them to specific exit codes

**[Models/](Models/)** - Configuration and enums
- `OutputFormat` enum: Sql, Csv, Json, Excel
- `CommandLineOptions`: Parses backward-compatible (3 args) and new named argument formats (--format, --output)

**[Formatters/](Formatters/)** - Format conversion strategy pattern
- `ITableFormatter` interface: All formatters implement `Format(data, output)` and `RequiresFileOutput` property
- `SqlFormatter`: Generates SQL INSERT with Table Values Constructor format (chunks at 500 rows)
- `CsvFormatter`: RFC 4180 compliant CSV using CsvHelper
- `JsonFormatter`: Pretty-printed JSON array of objects
- `ExcelFormatter`: .xlsx format using ClosedXML (requires file output)

**[SqlUtil.cs](SqlUtil.cs)** - SQL value conversion utilities
- `GetSqlValue()`: Converts .NET types to SQL-compatible string representations using `CultureInfo.InvariantCulture`
  - Handles: null/DBNull, strings (with SQL injection escaping), DateTime/DateTimeOffset/TimeSpan, numeric types, Guid, Boolean, byte arrays
  - Special handling: NaN/Infinity double/float values convert to NULL
- `EscapeSqlString()`: Escapes single quotes for SQL
- `GetColumnName()`: Wraps column names in square brackets
- Helper methods for generating SQL rows/columns

### Data Flow

1. **Input**: Command-line arguments (server, database, query, optional format/output)
2. **Query Execution**: Opens SQL connection and executes query via Dapper (returns dynamic objects as `IDictionary<string, object>`)
3. **Format Selection**: Creates appropriate formatter instance
4. **Validation**: Checks if formatter requires file output (Excel does)
5. **Output**: Writes formatted data to Console.Out or StreamWriter (file)

### Key Dependencies
- **Dapper 2.1.15**: Lightweight ORM for SQL execution
- **System.Data.SqlClient 4.8.6**: SQL Server connectivity
- **CsvHelper 33.0.1**: CSV format generation
- **ClosedXML 0.102.3**: Excel .xlsx generation
- **System.Text.Json**: Built-in JSON serialization

## Exit Codes

- `1`: Invalid arguments or parse failure
- `2`: SQL Server connection/execution error
- `3`: Query returned no results
- `4`: Excel format without --output parameter
- `5`: File I/O error (write failure)
- `6`: File access denied
- `7`: Invalid argument value
- `99`: Unexpected error

## Adding New Output Formats

1. Create formatter class in [Formatters/](Formatters/) implementing `ITableFormatter`
2. Set `RequiresFileOutput` property (true for formats like Excel that need file paths)
3. Implement `Format(IEnumerable<IDictionary<string, object>> data, TextWriter output)` method
4. Add enum value to `OutputFormat` in [Models/OutputFormat.cs](Models/OutputFormat.cs)
5. Update `CreateFormatter()` switch statement in [Program.cs](Program.cs)
6. Add tests in [TableConverter.Tests/Formatters/](TableConverter.Tests/Formatters/)

## Test Organization

Tests use xUnit and are organized by component:
- `SqlUtilTests.cs`: Tests for value conversion, SQL generation, locale handling
- `Formatters/SqlFormatterTests.cs`: SQL INSERT generation, Table Values Constructor chunking
- `Formatters/CsvFormatterTests.cs`: CSV output validation
- `Formatters/JsonFormatterTests.cs`: JSON formatting
- `Formatters/ExcelFormatterTests.cs`: Excel generation

## Important Implementation Details

### Locale Independence
All value conversions use `CultureInfo.InvariantCulture` to ensure consistent output regardless of system locale. This is critical for SQL Server operations where locale-specific formatting (e.g., decimal separators) would break queries.

### SQL Injection Prevention
String values are escaped by replacing single quotes with double single quotes (`''`) and wrapped with `N'` prefix (Unicode). This is handled in `SqlUtil.EscapeSqlString()` and applied in `GetSqlValue()`.

### Special Float/Double Handling
NaN and Infinity values are converted to NULL in SQL since SQL Server doesn't have equivalent special values.

### Table Values Constructor Chunking
SQL formatter splits results into groups of 500 rows per INSERT statement to handle large result sets within SQL Server's limitations.

### Empty Result Sets
If query returns no results, an `InvalidOperationException` is thrown by Dapper and caught to exit with code 3.
