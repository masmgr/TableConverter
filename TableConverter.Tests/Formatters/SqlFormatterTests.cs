namespace TableConverter.Tests.Formatters;

using System;
using System.Collections.Generic;
using Xunit;
using TableConverter.Formatters;

public class SqlFormatterTests
{
    [Fact]
    public void Format_EmptyData_WritesComment()
    {
        // Arrange
        var formatter = new SqlFormatter();
        var data = new List<IDictionary<string, object>>();
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        Assert.Contains("-- No rows returned from query", result);
    }

    [Fact]
    public void Format_SingleRow_GeneratesSingleInsert()
    {
        // Arrange
        var formatter = new SqlFormatter();
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
        Assert.Contains("INSERT INTO @source_table", result);
        Assert.Contains("VALUES", result);
        var insertCount = CountOccurrences(result, "INSERT INTO");
        Assert.Equal(1, insertCount);
    }

    [Fact]
    public void Format_MultipleRowsUnder500_GeneratesSingleInsert()
    {
        // Arrange
        var formatter = new SqlFormatter();
        var data = GenerateTestData(100);
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        var insertCount = result.Split(new[] { "INSERT INTO" }, StringSplitOptions.None).Length - 1;
        Assert.Equal(1, insertCount);
    }

    [Fact]
    public void Format_MultipleRowsOver500_GeneratesMultipleInserts()
    {
        // Arrange
        var formatter = new SqlFormatter();
        var data = GenerateTestData(501);
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        var insertCount = result.Split(new[] { "INSERT INTO" }, StringSplitOptions.None).Length - 1;
        Assert.Equal(2, insertCount);
    }

    [Fact]
    public void Format_Exactly500Rows_GeneratesSingleInsert()
    {
        // Arrange
        var formatter = new SqlFormatter();
        var data = GenerateTestData(500);
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        var insertCount = result.Split(new[] { "INSERT INTO" }, StringSplitOptions.None).Length - 1;
        Assert.Equal(1, insertCount);
    }

    [Fact]
    public void Format_1000Rows_GeneratesTwoInserts()
    {
        // Arrange
        var formatter = new SqlFormatter();
        var data = GenerateTestData(1000);
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        var insertCount = result.Split(new[] { "INSERT INTO" }, StringSplitOptions.None).Length - 1;
        Assert.Equal(2, insertCount);
    }

    [Fact]
    public void Format_NullValue_OutputsNull()
    {
        // Arrange
        var formatter = new SqlFormatter();
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
        Assert.Contains("NULL", result);
    }

    [Fact]
    public void Format_StringWithSpecialChars_EscapesProper()
    {
        // Arrange
        var formatter = new SqlFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Name", "O'Reilly" }
            }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        // SQL escaping of single quote
        Assert.Contains("O''Reilly", result);
    }

    [Fact]
    public void Format_BooleanValue_OutputsOneOrZero()
    {
        // Arrange
        var formatter = new SqlFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Active", true }
            },
            new Dictionary<string, object>
            {
                { "Id", 2 },
                { "Active", false }
            }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        // True should be 1, False should be 0
        var lines = result.Split(Environment.NewLine);
        Assert.Contains(lines, l => l.Contains("1,") && l.Contains("1"));
        Assert.Contains(lines, l => l.Contains("2,") && l.Contains("0"));
    }

    [Fact]
    public void RequiresFileOutput_ReturnsFalse()
    {
        // Arrange
        var formatter = new SqlFormatter();

        // Act & Assert
        Assert.False(formatter.RequiresFileOutput);
    }

    private static List<IDictionary<string, object>> GenerateTestData(int rowCount)
    {
        var data = new List<IDictionary<string, object>>();
        for (int i = 0; i < rowCount; i++)
        {
            data.Add(new Dictionary<string, object>
            {
                { "Id", i + 1 },
                { "Name", $"Name{i}" },
                { "Active", i % 2 == 0 }
            });
        }
        return data;
    }

    private static int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }
}
