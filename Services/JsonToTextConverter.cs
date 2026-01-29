using System.Text;
using System.Text.Json;

namespace FcrParse.Services;

public class JsonToTextConverter
{
    public static void ConvertJsonToText(string jsonFilePath, string outputFilePath)
    {
        // Read the JSON file
        var jsonContent = File.ReadAllText(jsonFilePath);
        var jsonDoc = JsonDocument.Parse(jsonContent);
        var root = jsonDoc.RootElement;

        var sb = new StringBuilder();

        // Process each property in the JSON
        foreach (var property in root.EnumerateObject())
        {
            var propertyName = property.Name;
            var value = property.Value;

            // Convert property name to readable format
            var displayName = FormatPropertyName(propertyName);
            sb.AppendLine($"{displayName}:");

            // Handle different value types
            if (value.ValueKind == JsonValueKind.Object)
            {
                // Handle nested objects (like ShipperInfo)
                foreach (var nestedProp in value.EnumerateObject())
                {
                    sb.AppendLine($"  {nestedProp.Name}: {nestedProp.Value.GetString() ?? "N/A"}");
                }
            }
            else if (value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in value.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        sb.AppendLine(item.GetString());
                    }
                    else
                    {
                        sb.AppendLine(item.ToString());
                    }
                }
            }
            else if (value.ValueKind == JsonValueKind.String)
            {
                sb.AppendLine(value.GetString());
            }
            else
            {
                sb.AppendLine(value.ToString());
            }

            sb.AppendLine(); // Add blank line between sections
        }

        // Write to output file
        File.WriteAllText(outputFilePath, sb.ToString());
        Console.WriteLine($"Converted JSON to text: {outputFilePath}");
    }

    private static string FormatPropertyName(string propertyName)
    {
        // Convert camelCase to readable format
        var sb = new StringBuilder();
        for (int i = 0; i < propertyName.Length; i++)
        {
            if (i > 0 && char.IsUpper(propertyName[i]))
            {
                sb.Append(' ');
            }
            if (i == 0)
            {
                sb.Append(char.ToUpper(propertyName[i]));
            }
            else
            {
                sb.Append(propertyName[i]);
            }
        }
        return sb.ToString();
    }
}
