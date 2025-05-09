namespace JANORM;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class EntityAttribute : Attribute
{
    public string TableName { get; }

    public EntityAttribute(string tableName) 
    {
        TableName = tableName;
    }
}

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class IdAttribute : Attribute 
{
    public string GenerationMethod { get; } = "AUTO_INCREMENT";

    public IdAttribute(string generationMethod) 
    {
        GenerationMethod = generationMethod;
    }
}