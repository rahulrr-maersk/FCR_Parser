namespace FcrParser.Services;

public static class ColumnExtractor
{
    public static List<string> ExtractColumn(string csvFilePath, string columnNamePattern)
    {
        var columnData = new List<string>();
        var lines = File.ReadAllLines(csvFilePath);
        
        if (lines.Length == 0) return columnData;
        
        int headerRowIndex = -1;
        int columnStart = -1;
        int columnEnd = -1;
        
        // Find the header row and determine column span
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            
            // Check if this line contains the column header
            if (line.Contains(columnNamePattern, StringComparison.OrdinalIgnoreCase))
            {
                headerRowIndex = i;
                
                // Find where the column name starts and ends in the line
                var columns = line.Split(',');
                for (int j = 0; j < columns.Length; j++)
                {
                    var cell = columns[j].Trim();
                    cell = System.Text.RegularExpressions.Regex.Replace(cell, @"x+", "", 
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
                    
                    if (!string.IsNullOrWhiteSpace(cell) && cell.Contains(columnNamePattern, StringComparison.OrdinalIgnoreCase))
                    {
                        columnStart = j;
                        
                        // Find the end of this column by looking for the next non-empty header
                        columnEnd = FindColumnEnd(columns, j);
                        break;
                    }
                }
                break;
            }
        }
        
        if (headerRowIndex == -1 || columnStart == -1)
        {
            return columnData;
        }
        
        // Extract all data from those columns (start after header row)
        for (int i = headerRowIndex + 1; i < lines.Length; i++)
        {
            var columns = lines[i].Split(',');
            
            // Check all columns in the range
            for (int j = columnStart; j <= Math.Min(columnEnd, columns.Length - 1); j++)
            {
                var value = columns[j];
                
                // Clean the value:
                // 1. Remove all 'x' markers
                value = System.Text.RegularExpressions.Regex.Replace(value, @"x+", "", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                // 2. Remove all double quotes (CSV artifacts)
                value = value.Replace("\"", "");
                
                // 3. Trim whitespace
                value = value.Trim();

                // 4. Replace multiple spaces with a slash separator
                value = System.Text.RegularExpressions.Regex.Replace(value, @"\s{2,}", " / ");
                
                // Skip empty values and column headers that shouldn't be in data
                if (!string.IsNullOrWhiteSpace(value) && !IsColumnHeader(value))
                {
                    columnData.Add(value);
                }
            }
        }
        
        return columnData;
    }
    
    private static int FindColumnEnd(string[] headerColumns, int startIndex)
    {
        // Start searching from the column after the current header
        for (int i = startIndex + 1; i < headerColumns.Length; i++)
        {
            var cell = headerColumns[i].Trim();
            
            // Remove 'x' markers to see actual content
            cell = System.Text.RegularExpressions.Regex.Replace(cell, @"x+", "", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
            
            // If we find another non-empty cell, that's the start of the next column
            if (!string.IsNullOrWhiteSpace(cell))
            {
                // Return the index just before the next header
                return i - 1;
            }
        }
        
        // If no next header found, use the rest of the row (or a reasonable max)
        // Use the smaller of: end of row or start + 20 columns
        return Math.Min(headerColumns.Length - 1, startIndex + 20);
    }
    
    private static bool IsColumnHeader(string value)
    {
        // Filter out column header names that shouldn't be in the extracted data
        var columnHeaders = new[]
        {
            "Cargo Description",
            "Marks and Numbers",
            "Marks & Numbers",
            "S/O Number",
            "Weight",
            "Measurement",
            "Gross Weight",
            "Nett Weight",
            "CBM"
        };
        
        return columnHeaders.Any(h => value.Equals(h, StringComparison.OrdinalIgnoreCase));
    }
    
    public static List<string> ExtractMarksAndNumbers(string csvFilePath)
    {
        return ExtractColumn(csvFilePath, "Marks");
    }
    
    public static List<string> ExtractCargoDescription(string csvFilePath)
    {
        return ExtractColumn(csvFilePath, "Cargo");
    }
}
