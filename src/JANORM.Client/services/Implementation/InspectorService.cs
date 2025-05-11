using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using JANORM.Core.attributes;
using JANORM.Core.definitions;

namespace JANORM.Client.services.Implementation;

public class InspectorService: IInspectorService
{

    public void InspectAssembly(Assembly assembly) 
    {
        foreach (var type in assembly.GetTypes())
        {   

             if (type.IsClass && type.GetCustomAttribute<EntityAttribute>() != null)
            {
                CreateEntity(type);
            }
        }

    }

    public void CreateEntity(Type type) {

        var entityAttribute = type.GetCustomAttribute<EntityAttribute>();
        var tableName = entityAttribute?.TableName ?? type.Name;
        Console.WriteLine($"Creating entity for table: {tableName}");
        var properties = type.GetProperties();
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
       
    }
}
