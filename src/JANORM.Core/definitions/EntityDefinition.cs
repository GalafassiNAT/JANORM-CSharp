using JANORM.Core.attributes;

namespace JANORM.Core.definitions;

public class EntityDefinition
{
    public string TableName { get; set; }
    public List<PropertyDefinition> Properties { get; set; } = new();

    public EntityDefinition(string tableName) {
        TableName = tableName;
    }

    public EntityDefinition(string tableName, List<PropertyDefinition> properties) {
        TableName = tableName;
        Properties = properties;
    }

}

public class PropertyDefinition
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool IsPrimaryKey { get; set; } = false;
    public bool IsNullable { get; set; } = true;
    public GenerationMethod? GenerationMethod { get; set; } = null;

    public PropertyDefinition(string name, string type, bool IsPrimaryKey, bool IsNullable, GenerationMethod? generationMethod = null)
    {
        Name = name;
        Type = type;
        this.IsPrimaryKey = IsPrimaryKey;
        this.IsNullable = IsNullable;
        GenerationMethod = generationMethod;
    }

}
