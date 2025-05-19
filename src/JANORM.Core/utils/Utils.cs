using System.Text.Json;


namespace JANORM.Core.utils;

public static class Utils
{
    public static string GetConnectionString()
    {

        SchemaFile schemaFile = GetSchemaFile();

        string rawConnectionString = schemaFile.Source.DatabaseUrl;
        string connectionString;

        if (string.IsNullOrEmpty(rawConnectionString))
        {
            throw new InvalidOperationException("Database URL is not specified in the schema file.");
        }

        if (rawConnectionString.StartsWith("env(") && rawConnectionString.EndsWith(")"))
        {
            string envVariable = rawConnectionString[4..^1];
            connectionString = Env(envVariable);
        }
        else
        {
            connectionString = rawConnectionString;
        }

        return connectionString;

    }

    public static SchemaFile GetSchemaFile()
    {
        string folder = Path.Combine(Directory.GetCurrentDirectory(), "JANORM");
        string schemaPath = Path.Combine(folder, "schema.jan");

        if (!File.Exists(schemaPath))
        {
            throw new FileNotFoundException($"Schema file not found at: {schemaPath}");
        }

        string jsonText = File.ReadAllText(schemaPath);
        SchemaFile schemaFile = JsonSerializer.Deserialize<SchemaFile>(jsonText)
            ?? throw new InvalidOperationException("Failed to deserialize schema file.");
        return schemaFile;
    }

    public static string MapToSqliteType(string csType)
    {
        Console.WriteLine($"Mapping C# type '{csType}' to SQLite type.");
        return csType.ToLower() switch
        {
            "int" => "INTEGER",
            "int64" => "INTEGER",
            "int32" => "INTEGER",
            "short" => "INTEGER",
            "long" => "INTEGER",
            "string" => "TEXT",
            "bool" => "BOOLEAN",
            "double" => "REAL",
            "single" => "REAL",
            "datetime" => "NUMERIC",
            "datetimeoffset" => "NUMERIC",
            "guid" => "TEXT",
            "byte" => "BLOB",
            _ => "TEXT"
        };
    }

    public static string Env(string value)
    {

        Console.WriteLine($"Fetching environment variable: {value}");

        string dbPath = Environment.GetEnvironmentVariable(value)
            ?? throw new InvalidOperationException($"Environment variable {value} not found.");

        if (dbPath.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
        {
            return dbPath;
        }

        return "Data Source=" + dbPath;
    }

}
