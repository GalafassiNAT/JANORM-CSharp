namespace JANORM.Core.attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class EntityAttribute : Attribute
{
    public string TableName { get; }

    public EntityAttribute(string tableName) 
    {
        TableName = tableName;
    }
}

public enum GenerationMethod
{
    AUTO_INCREMENT,
    UUID,
    NONE
}
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class IdAttribute : Attribute 
{
    public GenerationMethod? GenMethod { get; }

    public IdAttribute(GenerationMethod genMethod) 
    {
        GenMethod = genMethod;
    }
}
