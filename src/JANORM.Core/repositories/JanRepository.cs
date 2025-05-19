using JANORM.Core.attributes;
using JANORM.Core.definitions;
using JANORM.Core.services;
using JANORM.Core.services.Implementation;
using JANORM.Core.utils;

namespace JANORM.Core.repositories;

public static class JanRepository<T, U>
    where T : class
    where U : Type
{
    private static readonly IDBService _dbService;
    private static readonly EntityDefinition _entityDefinition;



    static JanRepository()
    {

        string connectionString = Utils.GetConnectionString();
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.");
        }

        IDBFactory dbFactory = new SqliteConnectionFactory(connectionString);
        _dbService = new SqliteDBService(dbFactory);
        _entityDefinition = Utils.GetSchemaFile().GetEntityDefinition(typeof(T)) ?? throw new ArgumentNullException($"Entity definition for {typeof(T).Name} not found.");
    }

    public static async Task<T> Insert(T entity)
    {



        return await Task.FromResult(default(T));
    }

    public static async Task<T> Update(T entity, U id)
    {
        // Implement update logic here
        return await Task.FromResult(default(T));
    }

    public static async Task Delete(U id)
    {
        // Implement delete logic here
        await Task.CompletedTask;
    }

    public static async Task<T> FindOneById(U id)
    {
        // Implement get by id logic here
        return await Task.FromResult(default(T));
    }

    public static async Task<List<T>> GetAll()
    {
        // Implement get all logic here
        return await Task.FromResult(new List<T>());
    }

    public static async Task<T> FindOne(dynamic query)
    {
        return await Task.FromResult(default(T)) 
            ?? throw new NotImplementedException("FindOne method is not implemented.");
    }
}
