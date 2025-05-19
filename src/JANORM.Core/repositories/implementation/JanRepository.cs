using System.Data.Common;
using System.Reflection;
using JANORM.Core.attributes;
using JANORM.Core.definitions;
using JANORM.Core.services;
using JANORM.Core.services.Implementation;
using JANORM.Core.utils;
using Microsoft.Data.Sqlite;

namespace JANORM.Core.repositories.implementation;

public class JanRepository<T, TKey> : IRepository<T, TKey>
    where T : class
    where TKey : notnull
{
    private  readonly IDBService _dbService;
    private readonly EntityDefinition _meta;
    private readonly PropertyInfo _pkProperty;

    public JanRepository(IDBService dBService)
    {
        _dbService = dBService;
        _meta = Utils.GetSchemaFile()
            .GetEntityDefinition(typeof(T))
            ?? throw new ArgumentNullException($"Entity definition for {typeof(T).Name} not found.");

        _pkProperty = typeof(T).GetProperties()
            .First(p => p.GetCustomAttribute<IdAttribute>() != null);
    }


    public async Task<T> Insert(T entity)
    {
         string tableName = _meta.TableName;
        var allPropsMeta = _meta.Properties;
        var pkMeta = allPropsMeta.FirstOrDefault(p => p.IsPrimaryKey); 

        
        if (pkMeta != null && pkMeta.GenerationMethod == GenerationMethod.UUID)
        {
            object? currentPkValue = _pkProperty.GetValue(entity);
            if (currentPkValue == null || (currentPkValue is Guid g && g == Guid.Empty))
            {
                Guid newGuid = Guid.NewGuid();
                _pkProperty.SetValue(entity, newGuid);
            }
        }

        
        var propsToInsertMeta = allPropsMeta
            .Where(p => !(p.IsPrimaryKey && p.GenerationMethod == GenerationMethod.AUTO_INCREMENT))
            .ToList();

        string sql;
        var dbParams = new List<DbParameter>();

        if (!propsToInsertMeta.Any())
        {
            
            sql = $"INSERT INTO \"{tableName}\" DEFAULT VALUES;";
        }
        else
        {
         
            var columnNames = propsToInsertMeta.Select(p => $"\"{p.Name}\""); 
            var paramNames = propsToInsertMeta.Select((p, i) => $"@param{i}");

            sql = $@"
                INSERT INTO ""{tableName}"" 
                    ({string.Join(", ", columnNames)})
                VALUES 
                    ({string.Join(", ", paramNames)});
            ";

            for (int i = 0; i < propsToInsertMeta.Count; i++)
            {
                var propMeta = propsToInsertMeta[i];

                var propInfo = typeof(T).GetProperty(propMeta.Name)!; 
                object? value = propInfo.GetValue(entity) ?? DBNull.Value;
                dbParams.Add(new SqliteParameter($"@param{i}", value)); 
            }
        }

        await _dbService.ExecuteNonQueryAsync(sql, dbParams.ToArray());


        if (pkMeta != null && pkMeta.GenerationMethod == GenerationMethod.AUTO_INCREMENT)
        {
            object? lastIdRaw = await _dbService.ExecuteQueryAsync("SELECT last_insert_rowid();");
            if (lastIdRaw != null && lastIdRaw != DBNull.Value)
            {
                var convertedPk = Convert.ChangeType(lastIdRaw, _pkProperty.PropertyType);
                _pkProperty.SetValue(entity, convertedPk);
            }
        }

        return entity;
    }

    public async Task<T> Update(T entity, TKey id)
    {
        // Implement update logic here
        return await Task.FromResult(default(T)) ??
            throw new NotImplementedException("Update method is not implemented.");
    }

    public async Task Delete(TKey id)
    {
        // Implement delete logic here
        await Task.CompletedTask;
    }

    public async Task<T> FindById(TKey id)
    {
        // Implement get by id logic here
        return await Task.FromResult(default(T)) ??
            throw new NotImplementedException("FindById method is not implemented.");
    }

    public async Task<List<T>> FindAll()
    {
        // Implement get all logic here
        return await Task.FromResult(new List<T>());
    }

    public async Task<T> FindOne(dynamic query)
    {
        return await Task.FromResult(default(T)) 
            ?? throw new NotImplementedException("FindOne method is not implemented.");
    }
}
