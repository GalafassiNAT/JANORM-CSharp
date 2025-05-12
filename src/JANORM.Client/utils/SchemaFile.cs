using System.Text.Json.Serialization;
using JANORM.Core.definitions;

namespace JANORM.Client.utils;

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

}
