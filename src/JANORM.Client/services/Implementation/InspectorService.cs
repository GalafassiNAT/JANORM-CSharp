using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JANORM.Client.utils;
using JANORM.Core.attributes;
using JANORM.Core.definitions;

namespace JANORM.Client.services.Implementation;

public class InspectorService: IInspectorService
{

    public void InspectAssembly(Assembly assembly, string path) 
    {
        foreach (var type in assembly.GetTypes())
        {   

             if (type.IsClass && type.GetCustomAttribute<EntityAttribute>() != null)
            {
                CreateEntity(type, path);
            }
        }

    }

    public void CreateEntity(Type type, string path) {

        if (type == null) 
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (string.IsNullOrEmpty(path)) 
        {
            throw new ArgumentNullException(nameof(path));
        }

        var entityAttribute = type.GetCustomAttribute<EntityAttribute>();
        var tableName = entityAttribute?.TableName ?? type.Name;
        Console.WriteLine($"Creating entity for table: {tableName}");
        var properties = type.GetProperties();
        Console.WriteLine($"Properties: {string.Join(", ", properties.Select(p => p.Name))}");


        var propertyDefinitions = new List<PropertyDefinition>();
        foreach (var property in properties) 
        {   

            var propName = property.Name;
            var propType = property.PropertyType.Name;
            var isPrimaryKey = property.GetCustomAttribute<IdAttribute>() != null;
            var isNullable = true; 
            GenerationMethod genMethod;
            if (isPrimaryKey) 
            {
                var idAttribute = property.GetCustomAttribute<IdAttribute>();
                genMethod = idAttribute?.GenMethod ?? GenerationMethod.NONE;
            }
            else 
            {
                genMethod = GenerationMethod.NONE;
            }

            propertyDefinitions.Add(new PropertyDefinition(propName, propType, isPrimaryKey, isNullable, genMethod));
        }
       
        EntityDefinition entityDefinition = new(tableName, propertyDefinitions);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        string jsontext = File.ReadAllText(path);
        SchemaFile schemaFile = JsonSerializer.Deserialize<SchemaFile>(jsontext, options) ?? throw new InvalidOperationException("Failed to deserialize schema file.");

        var existing = schemaFile.Entities.FirstOrDefault(e => e.TableName == tableName);
        if (existing is null)
        {
            schemaFile.Entities.Add(entityDefinition);
        }
        else
        {
            existing.Properties = entityDefinition.Properties;
        }

        string json = JsonSerializer.Serialize(schemaFile, options);
        File.WriteAllText(path, json);
        Console.WriteLine($"Entity {tableName} created and added to schema file at: {path}");
        

    }
}
