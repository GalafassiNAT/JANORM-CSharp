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
            object? lastIdRaw = await _dbService.ExecuteScalarAsync("SELECT last_insert_rowid();");
            if (lastIdRaw != null && lastIdRaw != DBNull.Value)
            {
                Type targetType = _pkProperty.PropertyType;
                Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                var convertedPk = Convert.ChangeType(lastIdRaw, underlyingType);
                _pkProperty.SetValue(entity, convertedPk);
            }
        }

        return entity;
    }

    public async Task<T> Update(T entity, TKey id)
    {
        string tableName = _meta.TableName;
        var allProps = _meta.Properties;

        var propsToUpdate = allProps
            .Where(p => !p.IsPrimaryKey)
            .ToList();

        if (!propsToUpdate.Any())
        {
            Console.WriteLine("No properties to update.");
            return entity;
        }

        var setClauses = new List<string>();
        var dbParams = new List<DbParameter>();
        int paramIndex = 0;

        foreach (var prop in propsToUpdate)
        {
            string columnName = prop.Name;
            string paramName = $"@param{paramIndex}";
            setClauses.Add($"\"{columnName}\" = {paramName}");

            var propInfo = typeof(T).GetProperty(prop.Name)!;
            object? value = propInfo.GetValue(entity) ?? DBNull.Value;
            dbParams.Add(new SqliteParameter(paramName, value));
            paramIndex++;
        }

        string pkColumnName = _pkProperty.Name;
        string pkParamName = $"@pkId";

        object pkParameterValue = id;
        if (id is Guid guidId)
        {
            pkParameterValue = guidId.ToString().ToUpperInvariant(); 
        }

        dbParams.Add(new SqliteParameter(pkParamName, pkParameterValue));

        string sql = $@"
            UPDATE ""{tableName}""
            SET {string.Join(", ", setClauses)}
            WHERE ""{pkColumnName}"" = {pkParamName};
        ";

        await _dbService.ExecuteNonQueryAsync(sql, dbParams.ToArray());
        return entity;

    }

    public async Task Delete(TKey id)
    {
        string tableName = _meta.TableName;
        string pkColumnName = _pkProperty.Name;

        string sql = $@"
            DELETE FROM ""{tableName}""
            WHERE ""{pkColumnName}"" = @id;
        ";
        
        object parameterValue = id;
        if (id is Guid guidId)
        {
            parameterValue = guidId.ToString().ToUpperInvariant(); 
        }

        var dbParams = new List<DbParameter>
        {
            new SqliteParameter("@id", parameterValue)
        };

        await _dbService.ExecuteNonQueryAsync(sql, dbParams.ToArray());
    }

    public async Task<T?> FindById(TKey id)
    {
        string tableName = _meta.TableName;
        string pkColumnName = _pkProperty.Name;
        string sql = $@"
            SELECT * FROM ""{tableName}""
            WHERE ""{pkColumnName}"" = @id;
        ";
        object parameterValue = id;
        if (id is Guid guidId)
        {
            parameterValue = guidId.ToString().ToUpperInvariant();       
        }

        var dbParams = new List<DbParameter>
        {
            new SqliteParameter("@id", parameterValue)
        };

        var result = await _dbService.ExecuteQueryAsync(sql, dbParams.ToArray());
        if (result.Count == 0)
        {
            return null;
        }

        var row = result[0];
        var entity = Activator.CreateInstance<T>();

        foreach (var prop in _meta.Properties)
        {
            if (row.TryGetValue(prop.Name, out object? dbValue)
                && dbValue != DBNull.Value)
            {
                var propInfo = typeof(T).GetProperty(prop.Name);
                if (propInfo != null && propInfo.CanWrite)
                {
                    Type targetType = propInfo.PropertyType;
                    Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                    try
                    {
                        object convertedValue;
                        if (underlyingType == typeof(Guid) && dbValue is string guidString)
                        {
                            convertedValue = Guid.Parse(guidString);
                        }
                        else
                        {
                            convertedValue = Convert.ChangeType(dbValue, underlyingType);
                        }
                        propInfo.SetValue(entity, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error converting value for property '{prop.Name}': {ex.Message}");
                        throw new InvalidOperationException($"Failed to convert value for property '{prop.Name}', TargetType: '{underlyingType.Name} not reached", ex);
                    }
                }
            }
        }

        return entity;
    }

    public async Task<List<T>> FindAll()
    {
        string tableName = _meta.TableName;
        string sql = $@"
            SELECT * FROM ""{tableName}"";
        ";

        var result = await _dbService.ExecuteQueryAsync(sql);
        var entities = new List<T>();

        foreach (var row in result)
        {
            var entity = Activator.CreateInstance<T>();

            foreach (var prop in _meta.Properties)
            {
               if (row.TryGetValue(prop.Name, out object? dbValue) && dbValue != DBNull.Value) // Usar TryGetValue
                {
                    var propInfo = typeof(T).GetProperty(prop.Name);
                    if (propInfo != null && propInfo.CanWrite)
                    {
                        Type targetType = propInfo.PropertyType;
                        Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                        try
                        {
                            object convertedValue;
                            if (underlyingType == typeof(Guid) && dbValue is string stringGuid)
                            {
                                convertedValue = Guid.Parse(stringGuid);
                            }
                            else
                            {
                                convertedValue = Convert.ChangeType(dbValue, underlyingType);
                            }
                            propInfo.SetValue(entity, convertedValue);
                        }
                        catch (Exception ex)
                        {
                             Console.WriteLine($"Error converting value for property {prop.Name} in FindAll. DB Value: '{dbValue}' (Type: {dbValue?.GetType().Name}). Target Type: '{underlyingType.Name}'. Error: {ex.Message}");
                        }
                    }
                }
            }

            entities.Add(entity);
        }

        return entities;
    }

    public async Task<T?> FindOne(Dictionary<string, object> query)
    {
        if (query == null || !query.Any())
        {
            throw new ArgumentException("query dictionary cannot be null or empty for FindOne.", nameof(query));
        }

        string tableName = _meta.TableName;
        var dbParams = new List<DbParameter>();
        var whereClauses = new List<string>();
        int paramIndex = 0;

        foreach (var condition in query)
        {
            var propMeta = _meta.Properties.FirstOrDefault(p => p.Name.Equals(condition.Key, StringComparison.OrdinalIgnoreCase)) ?? throw new ArgumentException($"Invalid column name '{condition.Key}' for entity {typeof(T).Name}.");
            string columnName = propMeta.Name; 
            string paramName = $"@param{paramIndex}";

            whereClauses.Add($"\"{columnName}\" = {paramName}");

            object paramValue = condition.Value;
            if (condition.Value is Guid guidValue && propMeta.Type == "UUID")
            {
                paramValue = guidValue.ToString().ToUpperInvariant(); 
            }

            dbParams.Add(new SqliteParameter(paramName, paramValue ?? DBNull.Value));
            paramIndex++;
        }

        string sql = $@"
            SELECT * FROM ""{tableName}""
            WHERE {string.Join(" AND ", whereClauses)}
            LIMIT 1; 
        "; 

        var result = await _dbService.ExecuteQueryAsync(sql, dbParams.ToArray());

        if (result.Count == 0)
        {
            return null; 
        }

        var row = result[0];
        var entity = Activator.CreateInstance<T>();

        foreach (var propMetaInSchema in _meta.Properties) 
        {
            if (row.TryGetValue(propMetaInSchema.Name, out object? dbValue) && dbValue != DBNull.Value)
            {
                var propInfo = typeof(T).GetProperty(propMetaInSchema.Name);
                if (propInfo != null && propInfo.CanWrite)
                {
                    Type targetType = propInfo.PropertyType;
                    Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                    try
                    {
                        object convertedValue;
                        if (underlyingType == typeof(Guid) && dbValue is string stringGuid)
                        {
                            convertedValue = Guid.Parse(stringGuid);
                        }
                        else
                        {
                            convertedValue = Convert.ChangeType(dbValue, underlyingType);
                        }
                        propInfo.SetValue(entity, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error converting value for property '{propMetaInSchema.Name}' in FindOne. DB Value: '{dbValue}' (Type: {dbValue?.GetType().Name}). Target Type: '{underlyingType.Name}'. Error: {ex.Message}");
                        throw new InvalidOperationException($"Failed to convert value for property '{propMetaInSchema.Name}', TargetType: '{underlyingType.Name} not reached", ex);
                    }
                }
            }
        }
        return entity;
    }
}
