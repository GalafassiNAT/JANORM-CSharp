using System.Reflection;

namespace JANORM;

public class InspectorService: IInspectorService
{

    public void inspectAssembly(Assembly assembly) 
    {
        foreach (var type in assembly.GetTypes())
        {
            createEntity(type);
        }

    }

    public void createEntity(Type type) {


        if (type.IsClass && type.GetCustomAttribute<EntityAttribute>() != null)
        {
            
        }
    }
}
