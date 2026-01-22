using System.Text;
using System.Text.RegularExpressions;

namespace FcrParser.Services;

public static class TextCleaner
{
    public static string Clean(string filePath)
    {
        var sb = new StringBuilder();
        
        // Use FileShare.ReadWrite to avoid crashes if the file is open in Excel
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            // 1. Remove the "x" markers from the visual template
            var clean = line.Replace("x,", "").Replace(",x", "");
            
            // 2. Collapse sequences of 3+ commas into a single space
            // This preserves separation between fields but removes empty grid cells
            clean = Regex.Replace(clean, ",{3,}", " ");
            
            // 3. Only keep lines that have actual content (letters/numbers)
            if (clean.Any(c => char.IsLetterOrDigit(c)))
            {
                sb.AppendLine(clean.Trim());
            }
        }
        return sb.ToString();
    }
}