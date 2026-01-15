namespace TableConverter.Tests.Formatters;

using System;
using System.Collections.Generic;
using Xunit;
using TableConverter.Formatters;

public class CsvFormatterTests
{
    [Fact]
    public void Format_EmptyData_WritesNothing()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var data = new List<IDictionary<string, object>>();
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString().Trim();
        Assert.Empty(result);
    }

    [Fact]
    public void Format_SingleRow_WritesHeaderAndData()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Name", "John" }
            }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length); // Header + 1 data row
        Assert.Contains("Id", lines[0]);
        Assert.Contains("Name", lines[0]);
    }

    [Fact]
    public void Format_MultipleRows_WritesAllRows()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { { "Id", 1 }, { "Name", "John" } },
            new Dictionary<string, object> { { "Id", 2 }, { "Name", "Jane" } },
            new Dictionary<string, object> { { "Id", 3 }, { "Name", "Bob" } }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(4, lines.Length); // Header + 3 data rows
    }

    [Fact]
    public void Format_FieldWithComma_QuotesField()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Name", "Smith, John" }
            }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        Assert.Contains("\"Smith, John\"", result);
    }

    [Fact]
    public void Format_FieldWithDoubleQuote_EscapesQuote()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Name", "Say \"Hello\"" }
            }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        // CsvHelper escapes double quotes by doubling them
        Assert.Contains("\"Say \"\"Hello\"\"\"", result);
    }

    [Fact]
    public void Format_FieldWithNewline_QuotesField()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Name", "Line1\nLine2" }
            }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        Assert.Contains("\"Line1", result);
    }

    [Fact]
    public void Format_NullValue_WritesEmpty()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Name", null }
            }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        // Second line should have "1," (for Id) followed by empty field for Name
        Assert.Contains("1", lines[1]);
    }

    [Fact]
    public void Format_DateTime_FormatsAsIso8601()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Created", new DateTime(2024, 1, 15, 14, 30, 45, 123) }
            }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        Assert.Contains("2024-01-15T14:30:45.123", result);
    }

    [Fact]
    public void Format_Boolean_OutputsOneOrZero()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { { "Id", 1 }, { "Active", true } },
            new Dictionary<string, object> { { "Id", 2 }, { "Active", false } }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        Assert.Contains("1", lines[1]); // true = 1
        Assert.Contains("0", lines[2]); // false = 0
    }

    [Fact]
    public void Format_Guid_FormatsAsString()
    {
        // Arrange
        var testGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var formatter = new CsvFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Id", 1 },
                { "UniqueId", testGuid }
            }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        Assert.Contains("12345678-1234-1234-1234-123456789012", result);
    }

    [Fact]
    public void Format_ByteArray_FormatsAsHexString()
    {
        // Arrange
        var formatter = new CsvFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Data", new byte[] { 0x01, 0x02, 0xFF } }
            }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        Assert.Contains("0102FF", result);
    }

    [Fact]
    public void RequiresFileOutput_ReturnsFalse()
    {
        // Arrange
        var formatter = new CsvFormatter();

        // Act & Assert
        Assert.False(formatter.RequiresFileOutput);
    }
}
