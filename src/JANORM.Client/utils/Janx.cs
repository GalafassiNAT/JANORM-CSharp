using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JANORM.Client.services;
using JANORM.Core.services;
using JANORM.Core.services.Implementation;
using JANORM.Core.attributes;
using JANORM.Core.definitions;
using JANORM.Core.utils;
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

    public static async Task Push()
    {
        string connectionString = Utils.GetConnectionString();

        IDBFactory dBFactory = new SqliteConnectionFactory(connectionString);
        IDBService dbService = new SqliteDBService(dBFactory);

        SchemaFile schemaFile = Utils.GetSchemaFile();

        foreach (var entity in schemaFile.Entities)
        {
            string tableName = entity.TableName;
            var properties = entity.Properties;

            var sb = new StringBuilder();
            sb.Append($"CREATE TABLE IF NOT EXISTS \"{tableName}\" (");
            var propDefinitions = new List<string>();
            PropertyDefinition? pkHandler = null;

            foreach (var prop in properties)
            {
                string columnType = Utils.MapToSqliteType(prop.Type);
                string columnName = $"\"{prop.Name}\" {columnType}";

                if (prop.IsPrimaryKey)
                {
                    if (properties.Count(p => p.IsPrimaryKey) == 1)
                    {
                        columnName += " PRIMARY KEY";
                        if (prop.Type == "INTEGER" && prop.GenerationMethod == GenerationMethod.AUTO_INCREMENT)
                        {
                            columnName += " AUTOINCREMENT";
                        }
                        else if (prop.Type != "INTEGER" && prop.GenerationMethod == GenerationMethod.AUTO_INCREMENT)
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Warning: Primary key '{prop.Name}' is not of type INTEGER. Auto-increment will not be applied.");
                            Console.ResetColor();
                        }

                        pkHandler = prop;
                    }
                }
                if (!prop.IsNullable)
                {
                    columnName += " NOT NULL";
                }

                propDefinitions.Add(columnName);
            }
            sb.Append(string.Join(", ", propDefinitions));

            var pkProps = properties.Where(p => p.IsPrimaryKey && p != pkHandler).ToList();
            if (pkProps.Any())
            {
                var pkColumnNames = pkProps.Select(p => $"\"{p.Name}\"");
                sb.Append($", PRIMARY KEY ({string.Join(", ", pkColumnNames)})");
            }

            sb.Append(");");
            string createTableQuery = sb.ToString();
            Console.WriteLine($"Executing query: {createTableQuery}");

            await dbService.ExecuteNonQueryAsync(createTableQuery);
            Console.WriteLine($"Table {tableName} created");


            // var columns = properties.Select(p => $"{p.Name} {MapToSqliteType(p.Type)}");
            // string columnsString = string.Join(", ", columns);
            // if (properties.Any(p => p.IsPrimaryKey))
            // {
            //     var primaryKeyColumns = properties.Where(p => p.IsPrimaryKey).Select(p => p.Name);
            //     string primaryKeyString = string.Join(", ", primaryKeyColumns);
            //     columnsString += $", PRIMARY KEY ({primaryKeyString})";
            // }

            // string createTableQuery = $"CREATE TABLE IF NOT EXISTS {tableName} ({columnsString})";
            // using var command = connection.CreateCommand();

            // command.CommandText = createTableQuery;
            // command.ExecuteNonQuery();
            // Console.WriteLine($"Table {tableName} created");

        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Schema pushed to the database successfully.");
        Console.ResetColor();
    
    }

    
}
