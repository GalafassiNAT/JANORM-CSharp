using System.Reflection;

namespace JANORM;

public interface IInspectorService
{
    void inspectAssembly(Assembly assembly);
    void createEntity(Type type);

}
