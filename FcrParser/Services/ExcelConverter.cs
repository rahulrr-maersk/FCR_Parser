using OfficeOpenXml;
using System.Text;

namespace FcrParser.Services;

/// <summary>
/// Converts Excel (XLSX) files to CSV format
/// </summary>
public static class ExcelConverter
{
    static ExcelConverter()
    {
        // Set EPPlus license context (non-commercial use)
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    /// <summary>
    /// Converts an XLSX file to CSV format
    /// </summary>
    /// <param name="xlsxPath">Path to the XLSX file</param>
    /// <param name="csvPath">Path where CSV should be saved</param>
    public static void ConvertToCsv(string xlsxPath, string csvPath)
    {
        using var package = new ExcelPackage(new FileInfo(xlsxPath));
        var worksheet = package.Workbook.Worksheets[0]; // First sheet only
        
        var csv = new StringBuilder();
        var rowCount = worksheet.Dimension?.Rows ?? 0;
        var colCount = worksheet.Dimension?.Columns ?? 0;

        for (int row = 1; row <= rowCount; row++)
        {
            var values = new List<string>();
            
            for (int col = 1; col <= colCount; col++)
            {
                var cell = worksheet.Cells[row, col];
                var value = cell.Value?.ToString() ?? string.Empty;
                
                // Escape quotes and wrap in quotes if contains comma, quote, or newline
                if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                {
                    value = $"\"{value.Replace("\"", "\"\"")}\"";
                }
                
                values.Add(value);
            }
            
            csv.AppendLine(string.Join(",", values));
        }

        File.WriteAllText(csvPath, csv.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// Checks if a file is an Excel file based on extension
    /// </summary>
    public static bool IsExcelFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension == ".xlsx" || extension == ".xls";
    }
}
