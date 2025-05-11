using System.Text.Json;
using System.Text.Json.Serialization;

namespace JANORM.Client.utils;

public class Janx
{
    public static void Init() {
        var folder = Path.Combine(Directory.GetCurrentDirectory(), "JANORM");
        Directory.CreateDirectory(folder);

        var source = new Source("env(DATABASE_URL)", "psql");
        var schemaFile = new SchemaFile(source);

        var options = new JsonSerializerOptions {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        var json = JsonSerializer.Serialize(schemaFile, options);
        
        var path = Path.Combine(folder, "schema.jan");
        File.WriteAllText(path, json);
        Console.WriteLine($"Schema file created at: {path}");
    }
}
