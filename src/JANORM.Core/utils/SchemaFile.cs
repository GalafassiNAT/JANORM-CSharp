using System.Reflection;
using System.Text.Json.Serialization;
using JANORM.Core.attributes;
using JANORM.Core.definitions;

namespace JANORM.Core.utils;

public class Source 
{
    public string DatabaseUrl { get; set; }
    public string Host { get; set; }

    public Source(string databaseUrl, string host)
    {
        DatabaseUrl = databaseUrl;
        Host = host;
    }
}

public class SchemaFile
{
    public Source Source { get; set; }
    public List<EntityDefinition> Entities { get; set; }

    [JsonConstructor]
    public SchemaFile(Source source, List<EntityDefinition> entities)
    {
        Source = source;
        Entities = entities;
    }
    
    public SchemaFile(Source source)
    {
        Source = source;
        Entities = new List<EntityDefinition>();
    }

    internal EntityDefinition? GetEntityDefinition(Type type)
    {
        if (type == null) 
        {
            throw new ArgumentNullException(nameof(type));
        }

        var entityAttribute = type.GetCustomAttribute<EntityAttribute>();
        var tableName = entityAttribute?.TableName ?? type.Name;

        var entityDefinition = Entities.FirstOrDefault(e => e.TableName == tableName);
        if (entityDefinition == null) 
        {
            entityDefinition = new EntityDefinition(tableName);
            Entities.Add(entityDefinition);
        }
        
        return entityDefinition;
    }
}
