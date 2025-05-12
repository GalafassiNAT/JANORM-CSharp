using System.Reflection;

namespace JANORM.Client.services;

public interface IInspectorService
{
    void InspectAssembly(Assembly assembly, string path);
    void CreateEntity(Type type, string path);

}
