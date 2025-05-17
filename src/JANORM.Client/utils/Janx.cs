using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JANORM.Client.services;
using JANORM.Client.services.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace JANORM.Client.utils;

public class Janx
{    
    public static void Init() {
        string folder = Path.Combine(Directory.GetCurrentDirectory(), "JANORM");
        Directory.CreateDirectory(folder);

        Source source = new("env(DATA_SOURCE)", "sqlite");
        SchemaFile schemaFile = new(source);

        JsonSerializerOptions options = new(){
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        string json = JsonSerializer.Serialize(schemaFile, options);
        
        string path = Path.Combine(folder, "schema.jan");
        File.WriteAllText(path, json);
        Console.WriteLine($"Schema file created at: {path}");
    }

    public static void Generate(IServiceProvider provider, string asmPath) 
    {
        string folder = Path.Combine(Directory.GetCurrentDirectory(), "JANORM");
        string path = Path.Combine(folder, "schema.jan");

        if (!File.Exists(path)) 
        {
            Console.WriteLine($"Schema file not found at: {path}");
            return;
        }

        if (!File.Exists(asmPath)) {
            Console.WriteLine($"Assembly file not found at: {asmPath}");
            return;
        }

        IInspectorService inspector = provider.GetRequiredService<IInspectorService>();

        Assembly asm = Assembly.LoadFrom(asmPath);

        inspector.InspectAssembly(asm, path);


    }

    public static void Push() 
    {
        string folder = Path.Combine(Directory.GetCurrentDirectory(), "JANORM");
        string schemaPath = Path.Combine(folder, "schema.jan");

        if (!File.Exists(schemaPath)) 
        {
            Console.WriteLine($"Schema file not found at: {schemaPath}");
            return;
        }

        string jsonText = File.ReadAllText(schemaPath);
        SchemaFile schemaFile = JsonSerializer.Deserialize<SchemaFile>(jsonText) 
            ?? throw new InvalidOperationException("Failed to deserialize schema file.");

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

        string dbDirectory = Path.GetDirectoryName(connectionString) 
            ?? throw new InvalidOperationException("Invalid database connection string.");
        if (!Directory.Exists(dbDirectory))
        {
            var adaptedPath = dbDirectory.Replace("Data Source=", string.Empty);
            Directory.CreateDirectory(adaptedPath.Split("/").First());

        }

        IDBFactory dBFactory = new SqliteConnectionFactory(connectionString);
        using var connection = dBFactory.CreateConnection();
        connection.Open();
        
        foreach (var entity in schemaFile.Entities)
        {
            string tableName = entity.TableName;
            var properties = entity.Properties;

            var columns = properties.Select(p => $"{p.Name} {MapToSqliteType(p.Type)}");
            string columnsString = string.Join(", ", columns);
            if (properties.Any(p => p.IsPrimaryKey))
            {
                var primaryKeyColumns = properties.Where(p => p.IsPrimaryKey).Select(p => p.Name);
                string primaryKeyString = string.Join(", ", primaryKeyColumns);
                columnsString += $", PRIMARY KEY ({primaryKeyString})";
            }

            string createTableQuery = $"CREATE TABLE IF NOT EXISTS {tableName} ({columnsString})";
            using var command = connection.CreateCommand();

            command.CommandText = createTableQuery;
            command.ExecuteNonQuery();
            Console.WriteLine($"Table {tableName} created or already exists.");

        }
    
    }

    public static string MapToSqliteType (string csType) 
    {
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
            "float" => "REAL",
            "datetime" => "NUMERIC",
            "datetimeoffset" => "NUMERIC",
            "guid" => "TEXT",
            "byte" => "BLOB",
            _ => "TEXT"
        };
    }

    public static string Env(string value)
    {

        string dbPath = Environment.GetEnvironmentVariable(value) 
            ?? throw new InvalidOperationException($"Environment variable {value} not found.");

        if (dbPath.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
        {
            return dbPath;
        }

        return "Data Source=" + dbPath;
    }
}
