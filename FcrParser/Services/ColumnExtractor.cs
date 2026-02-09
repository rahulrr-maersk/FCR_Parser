using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FcrParser.Services;

public static class ColumnExtractor
{
    private static List<string> ExtractColumn(string csvFilePath, string[] possibleColumnNames)
    {
        var columnData = new List<string>();
        
        using var reader = new StreamReader(csvFilePath);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false, // We'll find the header row manually
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null // Ignore bad data instead of throwing
        };
        
        using var csv = new CsvReader(reader, config);
        
        // Find the actual header row (look for row containing data column names)
        string[]? headerRecord = null;
        int headerRowIndex = -1;
        int currentRow = 0;
        
        while (csv.Read())
        {
            var row = new List<string>();
            for (int i = 0; i < csv.Parser.Count; i++)
            {
                row.Add(csv.GetField(i) ?? "");
            }
            
            // Check if this row contains any of our target column names
            var rowText = string.Join(" ", row);
            if (rowText.Contains("Marks", StringComparison.OrdinalIgnoreCase) || 
                rowText.Contains("Cargo", StringComparison.OrdinalIgnoreCase) ||
                rowText.Contains("S/O Number", StringComparison.OrdinalIgnoreCase))
            {
                headerRecord = row.ToArray();
                headerRowIndex = currentRow;
                break;
            }
            currentRow++;
        }
        
        if (headerRecord == null) return columnData;
        
        // Find column indices that match any of the possible names
        var matchingIndices = new List<int>();
        for (int i = 0; i < headerRecord.Length; i++)
        {
            var header = CleanHeaderName(headerRecord[i]);
            
            if (!string.IsNullOrWhiteSpace(header))
            {
                // Check if header matches any possible column name
                foreach (var possibleName in possibleColumnNames)
                {
                    if (header.Contains(possibleName, StringComparison.OrdinalIgnoreCase))
                    {
                        matchingIndices.Add(i);
                        
                        // Find span - add adjacent empty columns
                        for (int j = i + 1; j < headerRecord.Length; j++)
                        {
                            var nextHeader = CleanHeaderName(headerRecord[j]);
                            if (string.IsNullOrWhiteSpace(nextHeader))
                            {
                                matchingIndices.Add(j);
                            }
                            else
                            {
                                break; // Stop at next column header
                            }
                        }
                        goto ColumnFound; // Exit all loops
                    }
                }
            }
        }
        ColumnFound:
        
        if (matchingIndices.Count == 0) return columnData;
        
        bool lastRowWasEmpty = false;
        
        // Read data rows
        while (csv.Read())
        {
            bool isEmptyRow = true;
            var rowValues = new List<string>();
            
            // Check if this row is a page break marker or continuation header
            bool isPageBreak = false;
            bool isContinuationHeader = false;
            
            for (int i = 0; i < csv.Parser.Count; i++)
            {
                var cellValue = csv.GetField(i) ?? "";
                if (IsPageBreakMarker(cellValue))
                {
                    isPageBreak = true;
                }
                if (IsColumnHeader(cellValue))
                {
                    isContinuationHeader = true;
                }
            }
            
            // Skip page break markers and continuation headers, but continue reading
            if (isPageBreak || isContinuationHeader)
            {
                continue;
            }
            
            foreach (var index in matchingIndices)
            {
                var value = csv.GetField(index) ?? "";
                value = CleanCellValue(value);
                
                if (!string.IsNullOrWhiteSpace(value) && !IsColumnHeader(value) && !IsPageBreakMarker(value))
                {
                    rowValues.Add(value);
                    isEmptyRow = false;
                }
            }
            
            if (isEmptyRow && columnData.Count > 0 && !lastRowWasEmpty)
            {
                columnData.Add("---");
                lastRowWasEmpty = true;
            }
            else if (!isEmptyRow)
            {
                columnData.AddRange(rowValues);
                lastRowWasEmpty = false;
            }
        }
        
        // Remove trailing separators
        while (columnData.Count > 0 && columnData[columnData.Count - 1] == "---")
        {
            columnData.RemoveAt(columnData.Count - 1);
        }
        
        return columnData;
    }
    
    private static string CleanHeaderName(string header)
    {
        header = header.Trim();
        // Remove 'x' markers
        header = Regex.Replace(header, @"x+", "", RegexOptions.IgnoreCase).Trim();
        // Decode HTML entities like &amp; to &
        header = System.Net.WebUtility.HtmlDecode(header);
        return header;
    }
    
    private static string CleanCellValue(string value)
    {
        // Remove 'x' markers
        value = Regex.Replace(value, @"x+", "", RegexOptions.IgnoreCase);
        value = value.Trim();
        // Replace multiple spaces with separator
        value = Regex.Replace(value, @"\s{2,}", " / ");
        return value;
    }
    
    private static bool IsColumnHeader(string value)
    {
        var columnHeaders = new[]
        {
            "Cargo Description", "Marks and Numbers", "Marks & Numbers",
            "S/O Number", "Weight", "Measurement", "Gross Weight",
            "Nett Weight", "CBM", "Shipper", "Consignee",
            // Multi-page continuation headers
            "Marks and Numbers / Description continued",
            "Description continued",
            "continued"
        };
        
        return columnHeaders.Any(h => value.Contains(h, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Checks if a row indicates end of data section (but not end of document)
    /// </summary>
    private static bool IsPageBreakMarker(string value)
    {
        var markers = new[]
        {
            "CONTINUE ON NEXT PAGE",
            "Freight Collect",
            "Back to SO Form",
            "Click here to update"
        };
        
        return markers.Any(m => value.Contains(m, StringComparison.OrdinalIgnoreCase));
    }
    
    public static List<string> ExtractMarksAndNumbers(string csvFilePath)
    {
        var possibleNames = new[] { "Marks", "Mark", "M&N", "Marks & Numbers", "Marks and Numbers", "Marks & Nos" };
        return ExtractColumn(csvFilePath, possibleNames);
    }
    
    public static List<string> ExtractCargoDescription(string csvFilePath)
    {
        var possibleNames = new[] { "Cargo", "Description", "Cargo Description", "Goods Description", "Commodity" };
        return ExtractColumn(csvFilePath, possibleNames);
    }
    
    public static List<string> ExtractShipperInfo(string csvFilePath)
    {
        var possibleNames = new[] { "Shipper", "Shipper Name", "Shipper Address", "Consignor", "Shipper Details" };
        return ExtractColumn(csvFilePath, possibleNames);
    }
    
    public static List<string> ExtractConsigneeInfo(string csvFilePath)
    {
        var possibleNames = new[] { "Consignee", "Consignee Name", "Consignee Address", "Receiver" };
        return ExtractColumn(csvFilePath, possibleNames);
    }

    /// <summary>
    /// Returns column indices that should be excluded when cleaning CSV for AI processing
    /// (Marks & Numbers and Cargo Description columns with their spans)
    /// </summary>
    public static HashSet<int> GetColumnsToExclude(string csvFilePath)
    {
        var columnsToExclude = new HashSet<int>();
        
        // Column names to exclude
        var marksNames = new[] { "Marks", "Mark", "M&N", "Marks & Numbers", "Marks and Numbers", "Marks & Nos" };
        var cargoNames = new[] { "Cargo", "Cargo Description", "Goods Description", "Commodity" };
        
        using var reader = new StreamReader(csvFilePath);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null
        };
        
        using var csv = new CsvReader(reader, config);
        
        // Find the header row
        string[]? headerRecord = null;
        
        while (csv.Read())
        {
            var row = new List<string>();
            for (int i = 0; i < csv.Parser.Count; i++)
            {
                row.Add(csv.GetField(i) ?? "");
            }
            
            var rowText = string.Join(" ", row);
            if (rowText.Contains("Marks", StringComparison.OrdinalIgnoreCase) || 
                rowText.Contains("Cargo", StringComparison.OrdinalIgnoreCase) ||
                rowText.Contains("S/O Number", StringComparison.OrdinalIgnoreCase))
            {
                headerRecord = row.ToArray();
                break;
            }
        }
        
        if (headerRecord == null) return columnsToExclude;
        
        // Find Marks & Numbers column and its span
        for (int i = 0; i < headerRecord.Length; i++)
        {
            var header = CleanHeaderName(headerRecord[i]);
            
            if (!string.IsNullOrWhiteSpace(header))
            {
                // Check for Marks & Numbers
                foreach (var name in marksNames)
                {
                    if (header.Contains(name, StringComparison.OrdinalIgnoreCase))
                    {
                        columnsToExclude.Add(i);
                        // Add adjacent empty columns (span)
                        for (int j = i + 1; j < headerRecord.Length; j++)
                        {
                            var nextHeader = CleanHeaderName(headerRecord[j]);
                            if (string.IsNullOrWhiteSpace(nextHeader))
                            {
                                columnsToExclude.Add(j);
                            }
                            else
                            {
                                break;
                            }
                        }
                        break;
                    }
                }
                
                // Check for Cargo Description
                foreach (var name in cargoNames)
                {
                    if (header.Contains(name, StringComparison.OrdinalIgnoreCase))
                    {
                        columnsToExclude.Add(i);
                        // Add adjacent empty columns (span)
                        for (int j = i + 1; j < headerRecord.Length; j++)
                        {
                            var nextHeader = CleanHeaderName(headerRecord[j]);
                            if (string.IsNullOrWhiteSpace(nextHeader))
                            {
                                columnsToExclude.Add(j);
                            }
                            else
                            {
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        }
        
        return columnsToExclude;
    }
}
