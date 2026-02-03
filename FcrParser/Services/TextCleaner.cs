using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace FcrParser.Services;

public static class TextCleaner
{
    /// <summary>
    /// Cleans the CSV file by:
    /// 1. Excluding Marks & Numbers and Cargo Description columns (using ColumnExtractor logic)
    /// 2. Removing 'x' markers from the visual template
    /// 3. Collapsing excessive commas
    /// 4. Keeping only lines with actual content
    /// </summary>
    public static string Clean(string filePath)
    {
        var sb = new StringBuilder();
        
        // Get columns to exclude using the same logic as ColumnExtractor
        var columnsToExclude = ColumnExtractor.GetColumnsToExclude(filePath);
        
        // Now read the file again and process rows, excluding the identified columns
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null
        };
        
        using var csv = new CsvReader(reader, config);
        
        while (csv.Read())
        {
            var cleanedCells = new List<string>();
            
            for (int i = 0; i < csv.Parser.Count; i++)
            {
                // Skip excluded columns (Marks & Numbers, Cargo Description)
                if (columnsToExclude.Contains(i))
                {
                    continue;
                }
                
                var cell = csv.GetField(i) ?? "";
                
                // Remove 'x' markers
                cell = Regex.Replace(cell, @"^x+$", "", RegexOptions.IgnoreCase);
                cell = cell.Trim();
                
                if (!string.IsNullOrWhiteSpace(cell))
                {
                    cleanedCells.Add(cell);
                }
            }
            
            // Join cells and clean up
            if (cleanedCells.Count > 0)
            {
                var line = string.Join(",", cleanedCells);
                
                // Collapse sequences of 3+ commas into a single space
                line = Regex.Replace(line, ",{3,}", " ");
                
                // Only keep lines that have actual content (letters/numbers)
                if (line.Any(c => char.IsLetterOrDigit(c)))
                {
                    sb.AppendLine(line.Trim());
                }
            }
        }
        
        return sb.ToString();
    }
}