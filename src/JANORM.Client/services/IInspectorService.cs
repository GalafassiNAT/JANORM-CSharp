using System.Reflection;

namespace JANORM.Client.services;

public interface IInspectorService
{
    void InspectAssembly(Assembly assembly);
    void CreateEntity(Type type);

}
