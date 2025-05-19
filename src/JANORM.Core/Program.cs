using JANORM.Core.services;
using JANORM.Core.services.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace JANORM.Core;

public class Program
{
    public static void Main(string[] args)
    {
        ServiceCollection services = new();
        services.AddSingleton<IDBFactory, SqliteConnectionFactory>();
        services.AddTransient<IDBService, SqliteDBService>();


        // var dbFactory = new SqliteConnectionFactory("Data Source=database.db");
        // var dbService = new DBService(dbFactory);
        // var inspectorService = new InspectorService(dbService);
        // inspectorService.InspectAssembly(Assembly.GetExecutingAssembly(), "path/to/your/file.txt");
    }
}
