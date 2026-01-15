namespace TableConverter.Tests;

using System.Globalization;
using Xunit;

public class SqlUtilTests
{
    #region EscapeSqlString Tests

    [Fact]
    public void EscapeSqlString_WithSingleQuote_DoublesSingleQuote()
    {
        // Arrange
        var input = "O'Reilly";

        // Act
        var result = SqlUtil.EscapeSqlString(input);

        // Assert
        Assert.Equal("O''Reilly", result);
    }

    [Fact]
    public void EscapeSqlString_WithMultipleSingleQuotes_DoublesAllSingleQuotes()
    {
        // Arrange
        var input = "It's a 'test'";

        // Act
        var result = SqlUtil.EscapeSqlString(input);

        // Assert
        Assert.Equal("It''s a ''test''", result);
    }

    [Fact]
    public void EscapeSqlString_WithNoSingleQuote_ReturnsOriginal()
    {
        // Arrange
        var input = "Normal string";

        // Act
        var result = SqlUtil.EscapeSqlString(input);

        // Assert
        Assert.Equal("Normal string", result);
    }

    [Fact]
    public void EscapeSqlString_WithEmptyString_ReturnsEmptyString()
    {
        // Arrange
        var input = "";

        // Act
        var result = SqlUtil.EscapeSqlString(input);

        // Assert
        Assert.Equal("", result);
    }

    #endregion

    #region GetSqlValue - Null Tests

    [Fact]
    public void GetSqlValue_Null_ReturnsNULL()
    {
        // Act
        var result = SqlUtil.GetSqlValue(null);

        // Assert
        Assert.Equal("NULL", result);
    }

    [Fact]
    public void GetSqlValue_DBNull_ReturnsNULL()
    {
        // Act
        var result = SqlUtil.GetSqlValue(DBNull.Value);

        // Assert
        Assert.Equal("NULL", result);
    }

    #endregion

    #region GetSqlValue - String Tests

    [Fact]
    public void GetSqlValue_String_ReturnsQuotedString()
    {
        // Act
        var result = SqlUtil.GetSqlValue("test");

        // Assert
        Assert.Equal("N'test'", result);
    }

    [Fact]
    public void GetSqlValue_StringWithSingleQuote_EscapesQuote()
    {
        // Act
        var result = SqlUtil.GetSqlValue("O'Reilly");

        // Assert
        Assert.Equal("N'O''Reilly'", result);
    }

    [Fact]
    public void GetSqlValue_EmptyString_ReturnsEmptyQuotedString()
    {
        // Act
        var result = SqlUtil.GetSqlValue("");

        // Assert
        Assert.Equal("N''", result);
    }

    [Fact]
    public void GetSqlValue_StringWithSpecialCharacters_ReturnsQuotedString()
    {
        // Act
        var result = SqlUtil.GetSqlValue("test\"with\nnewline");

        // Assert
        Assert.Equal("N'test\"with\nnewline'", result);
    }

    #endregion

    #region GetSqlValue - DateTime Tests

    [Fact]
    public void GetSqlValue_DateTime_ReturnsISO8601Format()
    {
        // Arrange
        var date = new DateTime(2024, 1, 15, 14, 30, 45, 123);

        // Act
        var result = SqlUtil.GetSqlValue(date);

        // Assert
        Assert.Equal("'2024-01-15T14:30:45.123'", result);
    }

    [Fact]
    public void GetSqlValue_DateTime_IsCultureInvariant()
    {
        // Arrange
        var currentCulture = CultureInfo.CurrentCulture;
        var date = new DateTime(2024, 1, 15, 14, 30, 45, 123);

        try
        {
            // Set culture to Japanese (uses different date format)
            CultureInfo.CurrentCulture = new CultureInfo("ja-JP");

            // Act
            var result = SqlUtil.GetSqlValue(date);

            // Assert - should still be ISO 8601 format
            Assert.Equal("'2024-01-15T14:30:45.123'", result);
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
        }
    }

    [Fact]
    public void GetSqlValue_DateTimeAtMidnight_FormatsCorrectly()
    {
        // Arrange
        var date = new DateTime(2024, 1, 1, 0, 0, 0, 0);

        // Act
        var result = SqlUtil.GetSqlValue(date);

        // Assert
        Assert.Equal("'2024-01-01T00:00:00.000'", result);
    }

    #endregion

    #region GetSqlValue - DateTimeOffset Tests

    [Fact]
    public void GetSqlValue_DateTimeOffset_ReturnsISO8601WithTimeZone()
    {
        // Arrange
        var dateOffset = new DateTimeOffset(2024, 1, 15, 14, 30, 45, 123, TimeSpan.FromHours(9));

        // Act
        var result = SqlUtil.GetSqlValue(dateOffset);

        // Assert
        Assert.StartsWith("'2024-01-15T14:30:45.", result);
        Assert.EndsWith("+09:00'", result);
    }

    #endregion

    #region GetSqlValue - TimeSpan Tests

    [Fact]
    public void GetSqlValue_TimeSpan_ReturnsFormattedTimeSpan()
    {
        // Arrange
        var timespan = new TimeSpan(14, 30, 45);

        // Act
        var result = SqlUtil.GetSqlValue(timespan);

        // Assert
        Assert.StartsWith("'14:30:45.", result);
        Assert.EndsWith("'", result);
    }

    #endregion

    #region GetSqlValue - Boolean Tests

    [Fact]
    public void GetSqlValue_True_Returns1()
    {
        // Act
        var result = SqlUtil.GetSqlValue(true);

        // Assert
        Assert.Equal("1", result);
    }

    [Fact]
    public void GetSqlValue_False_Returns0()
    {
        // Act
        var result = SqlUtil.GetSqlValue(false);

        // Assert
        Assert.Equal("0", result);
    }

    #endregion

    #region GetSqlValue - Guid Tests

    [Fact]
    public void GetSqlValue_Guid_ReturnsQuotedGuid()
    {
        // Arrange
        var guid = new Guid("12345678-1234-1234-1234-123456789012");

        // Act
        var result = SqlUtil.GetSqlValue(guid);

        // Assert
        Assert.Equal("'12345678-1234-1234-1234-123456789012'", result);
    }

    [Fact]
    public void GetSqlValue_EmptyGuid_ReturnsQuotedEmptyGuid()
    {
        // Arrange
        var guid = Guid.Empty;

        // Act
        var result = SqlUtil.GetSqlValue(guid);

        // Assert
        Assert.Equal("'00000000-0000-0000-0000-000000000000'", result);
    }

    #endregion

    #region GetSqlValue - Numeric Tests

    [Fact]
    public void GetSqlValue_Decimal_PreservesPrecision()
    {
        // Arrange
        // decimal type has 28-29 significant digits; this tests that we preserve as much as possible
        var value = 123.45678901234567890123456789m;

        // Act
        var result = SqlUtil.GetSqlValue(value);

        // Assert
        // The exact representation depends on decimal's internal storage, but it should preserve significant digits
        Assert.StartsWith("123.456789", result);
        Assert.DoesNotContain("E", result); // Should not use scientific notation
    }

    [Fact]
    public void GetSqlValue_Decimal_UsesInvariantCulture()
    {
        // Arrange
        var currentCulture = CultureInfo.CurrentCulture;
        var value = 123.456m;

        try
        {
            // Set culture to German (uses comma for decimal separator)
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            // Act
            var result = SqlUtil.GetSqlValue(value);

            // Assert - should use period, not comma
            Assert.Contains(".", result);
            Assert.DoesNotContain(",", result);
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
        }
    }

    [Fact]
    public void GetSqlValue_Double_UsesInvariantCulture()
    {
        // Arrange
        var currentCulture = CultureInfo.CurrentCulture;
        var value = 123.456;

        try
        {
            // Set culture to German (uses comma for decimal separator)
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            // Act
            var result = SqlUtil.GetSqlValue(value);

            // Assert - should use period, not comma
            Assert.Contains(".", result);
            Assert.DoesNotContain(",", result);
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
        }
    }

    [Fact]
    public void GetSqlValue_Float_UsesInvariantCulture()
    {
        // Arrange
        var currentCulture = CultureInfo.CurrentCulture;
        float value = 123.456f;

        try
        {
            // Set culture to German (uses comma for decimal separator)
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");

            // Act
            var result = SqlUtil.GetSqlValue(value);

            // Assert - should use period, not comma
            Assert.Contains(".", result);
            Assert.DoesNotContain(",", result);
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
        }
    }

    [Fact]
    public void GetSqlValue_DoubleNaN_ReturnsNULL()
    {
        // Act
        var result = SqlUtil.GetSqlValue(double.NaN);

        // Assert
        Assert.Equal("NULL", result);
    }

    [Fact]
    public void GetSqlValue_DoublePositiveInfinity_ReturnsNULL()
    {
        // Act
        var result = SqlUtil.GetSqlValue(double.PositiveInfinity);

        // Assert
        Assert.Equal("NULL", result);
    }

    [Fact]
    public void GetSqlValue_DoubleNegativeInfinity_ReturnsNULL()
    {
        // Act
        var result = SqlUtil.GetSqlValue(double.NegativeInfinity);

        // Assert
        Assert.Equal("NULL", result);
    }

    [Fact]
    public void GetSqlValue_FloatNaN_ReturnsNULL()
    {
        // Act
        var result = SqlUtil.GetSqlValue(float.NaN);

        // Assert
        Assert.Equal("NULL", result);
    }

    [Theory]
    [InlineData(int.MaxValue, "2147483647")]
    [InlineData(int.MinValue, "-2147483648")]
    [InlineData(0, "0")]
    public void GetSqlValue_Integer_ReturnsStringValue(int value, string expected)
    {
        // Act
        var result = SqlUtil.GetSqlValue(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetSqlValue_Long_ReturnsStringValue()
    {
        // Act
        var result = SqlUtil.GetSqlValue(long.MaxValue);

        // Assert
        Assert.Equal(long.MaxValue.ToString(), result);
    }

    [Fact]
    public void GetSqlValue_Byte_ReturnsStringValue()
    {
        // Arrange
        byte value = 255;

        // Act
        var result = SqlUtil.GetSqlValue(value);

        // Assert
        Assert.Equal("255", result);
    }

    #endregion

    #region GetSqlValue - Binary Tests

    [Fact]
    public void GetSqlValue_ByteArray_ReturnsHexString()
    {
        // Arrange
        var bytes = new byte[] { 0x01, 0x02, 0x0A, 0xFF };

        // Act
        var result = SqlUtil.GetSqlValue(bytes);

        // Assert
        Assert.Equal("0x01020AFF", result);
    }

    [Fact]
    public void GetSqlValue_EmptyByteArray_Returns0x()
    {
        // Arrange
        var bytes = new byte[] { };

        // Act
        var result = SqlUtil.GetSqlValue(bytes);

        // Assert
        Assert.Equal("0x", result);
    }

    [Fact]
    public void GetSqlValue_SingleByteArray_ReturnsHexString()
    {
        // Arrange
        var bytes = new byte[] { 0xFF };

        // Act
        var result = SqlUtil.GetSqlValue(bytes);

        // Assert
        Assert.Equal("0xFF", result);
    }

    #endregion

    #region GetSqlValueRow Tests

    [Fact]
    public void GetSqlValueRow_CreatesParenthesizedList()
    {
        // Arrange
        var values = new[] { "1", "N'test'", "NULL" };

        // Act
        var result = SqlUtil.GetSqlValueRow(values);

        // Assert
        Assert.Equal("(1, N'test', NULL)", result);
    }

    [Fact]
    public void GetSqlValueRow_WithSingleValue_CreatesParenthesizedValue()
    {
        // Arrange
        var values = new[] { "42" };

        // Act
        var result = SqlUtil.GetSqlValueRow(values);

        // Assert
        Assert.Equal("(42)", result);
    }

    [Fact]
    public void GetSqlValueRow_WithEmptyList_CreatesEmptyParentheses()
    {
        // Arrange
        var values = new string[] { };

        // Act
        var result = SqlUtil.GetSqlValueRow(values);

        // Assert
        Assert.Equal("()", result);
    }

    #endregion

    #region GetColumnNames Tests

    [Fact]
    public void GetColumnNames_BracketsColumnNames()
    {
        // Arrange
        var row = new Dictionary<string, object>
        {
            { "Id", 1 },
            { "Name", "Test" },
            { "Created Date", DateTime.Now }
        };

        // Act
        var result = SqlUtil.GetColumnNames(row).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("[Id]", result);
        Assert.Contains("[Name]", result);
        Assert.Contains("[Created Date]", result);
    }

    [Fact]
    public void GetColumnNames_WithSpecialCharactersInColumnName_BracketsColumnNames()
    {
        // Arrange
        var row = new Dictionary<string, object>
        {
            { "Column [With] Brackets", 123 },
            { "Column With Spaces", "value" }
        };

        // Act
        var result = SqlUtil.GetColumnNames(row).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("[Column [With] Brackets]", result);
        Assert.Contains("[Column With Spaces]", result);
    }

    [Fact]
    public void GetColumnNames_WithEmptyDictionary_ReturnsEmptyList()
    {
        // Arrange
        var row = new Dictionary<string, object>();

        // Act
        var result = SqlUtil.GetColumnNames(row).ToList();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetColumnName Tests

    [Fact]
    public void GetColumnName_SimpleName_BracketsName()
    {
        // Act
        var result = SqlUtil.GetColumnName("Id");

        // Assert
        Assert.Equal("[Id]", result);
    }

    [Fact]
    public void GetColumnName_NameWithSpaces_BracketsName()
    {
        // Act
        var result = SqlUtil.GetColumnName("First Name");

        // Assert
        Assert.Equal("[First Name]", result);
    }

    #endregion
}
