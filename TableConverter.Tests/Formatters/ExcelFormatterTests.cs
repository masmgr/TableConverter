namespace TableConverter.Tests.Formatters;

using System;
using System.Collections.Generic;
using Xunit;
using ClosedXML.Excel;
using TableConverter.Formatters;

public class ExcelFormatterTests
{
    [Fact]
    public void Format_CreatesExcelFile()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
        var formatter = new ExcelFormatter(tempFile);
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Name", "John" }
            }
        };
        var output = new StringWriter();

        try
        {
            // Act
            formatter.Format(data, output);

            // Assert
            Assert.True(File.Exists(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Format_CreatesWorkbookWithDataWorksheet()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
        var formatter = new ExcelFormatter(tempFile);
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Name", "John" }
            }
        };
        var output = new StringWriter();

        try
        {
            // Act
            formatter.Format(data, output);

            // Assert
            using var workbook = new XLWorkbook(tempFile);
            var worksheet = workbook.Worksheet("Data");
            Assert.NotNull(worksheet);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Format_WritesHeadersInFirstRow()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
        var formatter = new ExcelFormatter(tempFile);
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Name", "John" }
            }
        };
        var output = new StringWriter();

        try
        {
            // Act
            formatter.Format(data, output);

            // Assert
            using var workbook = new XLWorkbook(tempFile);
            var worksheet = workbook.Worksheet("Data");
            Assert.Equal("Id", worksheet.Cell(1, 1).Value);
            Assert.Equal("Name", worksheet.Cell(1, 2).Value);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Format_HeadersAreBold()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
        var formatter = new ExcelFormatter(tempFile);
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { { "Id", 1 } }
        };
        var output = new StringWriter();

        try
        {
            // Act
            formatter.Format(data, output);

            // Assert
            using var workbook = new XLWorkbook(tempFile);
            var worksheet = workbook.Worksheet("Data");
            Assert.True(worksheet.Cell(1, 1).Style.Font.Bold);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Format_WritesDataStartingFromRow2()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
        var formatter = new ExcelFormatter(tempFile);
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Name", "John" }
            }
        };
        var output = new StringWriter();

        try
        {
            // Act
            formatter.Format(data, output);

            // Assert
            using var workbook = new XLWorkbook(tempFile);
            var worksheet = workbook.Worksheet("Data");
            Assert.Equal(1, worksheet.Cell(2, 1).Value);
            Assert.Equal("John", worksheet.Cell(2, 2).Value);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Format_EmptyData_CreatesEmptyWorkbook()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
        var formatter = new ExcelFormatter(tempFile);
        var data = new List<IDictionary<string, object>>();
        var output = new StringWriter();

        try
        {
            // Act
            formatter.Format(data, output);

            // Assert
            Assert.True(File.Exists(tempFile));
            using var workbook = new XLWorkbook(tempFile);
            var worksheet = workbook.Worksheet("Data");
            Assert.NotNull(worksheet);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Format_MultipleRows_WritesAllRows()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
        var formatter = new ExcelFormatter(tempFile);
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { { "Id", 1 } },
            new Dictionary<string, object> { { "Id", 2 } },
            new Dictionary<string, object> { { "Id", 3 } }
        };
        var output = new StringWriter();

        try
        {
            // Act
            formatter.Format(data, output);

            // Assert
            using var workbook = new XLWorkbook(tempFile);
            var worksheet = workbook.Worksheet("Data");
            Assert.Equal(1, worksheet.Cell(2, 1).Value);
            Assert.Equal(2, worksheet.Cell(3, 1).Value);
            Assert.Equal(3, worksheet.Cell(4, 1).Value);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Format_NullValue_WritesEmptyCell()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
        var formatter = new ExcelFormatter(tempFile);
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Id", 1 },
                { "Name", null }
            }
        };
        var output = new StringWriter();

        try
        {
            // Act
            formatter.Format(data, output);

            // Assert
            using var workbook = new XLWorkbook(tempFile);
            var worksheet = workbook.Worksheet("Data");
            // ClosedXML converts empty strings to empty cells
            var cellValue = worksheet.Cell(2, 2).Value;
            Assert.True(string.IsNullOrEmpty(cellValue.ToString()));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Format_DateTime_WritesAsExcelDate()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
        var formatter = new ExcelFormatter(tempFile);
        var testDate = new DateTime(2024, 1, 15, 14, 30, 45);
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Created", testDate }
            }
        };
        var output = new StringWriter();

        try
        {
            // Act
            formatter.Format(data, output);

            // Assert
            using var workbook = new XLWorkbook(tempFile);
            var worksheet = workbook.Worksheet("Data");
            var cellValue = worksheet.Cell(2, 1).Value;
            // ClosedXML returns XLCellValue which can be cast to DateTime
            var dtValue = (DateTime)cellValue;
            Assert.Equal(testDate.Date, dtValue.Date);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Format_Boolean_WritesOneOrZero()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
        var formatter = new ExcelFormatter(tempFile);
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object> { { "Active", true } },
            new Dictionary<string, object> { { "Active", false } }
        };
        var output = new StringWriter();

        try
        {
            // Act
            formatter.Format(data, output);

            // Assert
            using var workbook = new XLWorkbook(tempFile);
            var worksheet = workbook.Worksheet("Data");
            Assert.Equal(1L, worksheet.Cell(2, 1).Value);
            Assert.Equal(0L, worksheet.Cell(3, 1).Value);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Format_LargeDataset_WritesAllRows()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
        var formatter = new ExcelFormatter(tempFile);
        var data = new List<IDictionary<string, object>>();
        for (int i = 0; i < 1000; i++)
        {
            data.Add(new Dictionary<string, object> { { "Id", i + 1 } });
        }
        var output = new StringWriter();

        try
        {
            // Act
            formatter.Format(data, output);

            // Assert
            using var workbook = new XLWorkbook(tempFile);
            var worksheet = workbook.Worksheet("Data");
            // Header + 1000 data rows = 1001 rows total
            Assert.Equal(1001, worksheet.LastRowUsed().RowNumber());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Format_Guid_WritesAsString()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
        var formatter = new ExcelFormatter(tempFile);
        var testGuid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var data = new List<IDictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "UniqueId", testGuid }
            }
        };
        var output = new StringWriter();

        try
        {
            // Act
            formatter.Format(data, output);

            // Assert
            using var workbook = new XLWorkbook(tempFile);
            var worksheet = workbook.Worksheet("Data");
            Assert.Equal("12345678-1234-1234-1234-123456789012", worksheet.Cell(2, 1).Value);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void RequiresFileOutput_ReturnsTrue()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xlsx");
        var formatter = new ExcelFormatter(tempFile);

        // Act & Assert
        Assert.True(formatter.RequiresFileOutput);
    }
}
