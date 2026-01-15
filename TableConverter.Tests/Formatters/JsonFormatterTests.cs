namespace TableConverter.Tests.Formatters;

using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using TableConverter.Formatters;

public class JsonFormatterTests
{
    [Fact]
    public void Format_EmptyData_WritesEmptyArray()
    {
        // Arrange
        var formatter = new JsonFormatter();
        var data = new List<IDictionary<string, object>>();
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString().Trim();
        Assert.Equal("[]", result);
    }

    [Fact]
    public void Format_SingleRow_WritesArrayWithOneObject()
    {
        // Arrange
        var formatter = new JsonFormatter();
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
        var result = output.ToString().Trim();
        var json = JsonSerializer.Deserialize<List<JsonElement>>(result);
        Assert.NotNull(json);
        Assert.Single(json);
    }

    [Fact]
    public void Format_MultipleRows_WritesArrayWithMultipleObjects()
    {
        // Arrange
        var formatter = new JsonFormatter();
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
        var result = output.ToString().Trim();
        var json = JsonSerializer.Deserialize<List<JsonElement>>(result);
        Assert.NotNull(json);
        Assert.Equal(3, json.Count);
    }

    [Fact]
    public void Format_NullValue_WritesJsonNull()
    {
        // Arrange
        var formatter = new JsonFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Name", "John" },
                { "Age", null }
            }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        var json = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(result);
        Assert.NotNull(json);
        Assert.Equal(JsonValueKind.Null, json[0]["Age"].ValueKind);
    }

    [Fact]
    public void Format_DateTime_FormatsAsIso8601String()
    {
        // Arrange
        var formatter = new JsonFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
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
    public void Format_Boolean_WritesJsonBoolean()
    {
        // Arrange
        var formatter = new JsonFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { { "Active", true } },
            new Dictionary<string, object> { { "Active", false } }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        var json = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(result);
        Assert.NotNull(json);
        Assert.Equal(JsonValueKind.True, json[0]["Active"].ValueKind);
        Assert.Equal(JsonValueKind.False, json[1]["Active"].ValueKind);
    }

    [Fact]
    public void Format_Decimal_WritesAsString()
    {
        // Arrange
        var formatter = new JsonFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Amount", 123.456789m }
            }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        var json = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(result);
        Assert.NotNull(json);
        // Decimal is written as string to preserve precision
        Assert.Equal(JsonValueKind.String, json[0]["Amount"].ValueKind);
    }

    [Fact]
    public void Format_Integer_WritesAsNumber()
    {
        // Arrange
        var formatter = new JsonFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Count", 42 }
            }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString();
        var json = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(result);
        Assert.NotNull(json);
        Assert.Equal(JsonValueKind.Number, json[0]["Count"].ValueKind);
        Assert.Equal(42, json[0]["Count"].GetInt32());
    }

    [Fact]
    public void Format_Guid_WritesAsString()
    {
        // Arrange
        var testGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var formatter = new JsonFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
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
    public void Format_ByteArray_WritesAsHexString()
    {
        // Arrange
        var formatter = new JsonFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
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
    public void Format_ValidJson_CanBeDeserialized()
    {
        // Arrange
        var formatter = new JsonFormatter();
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Name", "Test" },
                { "Active", true },
                { "Value", 123.45m }
            }
        };
        var output = new StringWriter();

        // Act
        formatter.Format(data, output);

        // Assert
        var result = output.ToString().Trim();
        // Should not throw
        var json = JsonSerializer.Deserialize<List<JsonElement>>(result);
        Assert.NotNull(json);
        Assert.Single(json);
    }

    [Fact]
    public void RequiresFileOutput_ReturnsFalse()
    {
        // Arrange
        var formatter = new JsonFormatter();

        // Act & Assert
        Assert.False(formatter.RequiresFileOutput);
    }
}
