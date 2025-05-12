using System.Data.Common;

namespace JANORM.Client.services.Implementation;

public class SqliteDBService : IDBService
{   

    private readonly IDBFactory _dbFactory;
    
    public SqliteDBService(IDBFactory connProvider) 
    {
        _dbFactory = connProvider;
    }

    public async Task<int> ExecuteNonQueryAsync(string sql, params DbParameter[] parameters)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.OpenAsync();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(parameters);
        return await cmd.ExecuteNonQueryAsync();
    }

    public Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, params DbParameter[] parameters)
    {
        using var connection = _dbFactory.CreateConnection();
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(parameters);
        using var reader = cmd.ExecuteReader();
        var result = new List<Dictionary<string, object>>();
        while (reader.Read())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                row[columnName] = value ?? DBNull.Value; 
            }
            result.Add(row);
        }
        return Task.FromResult(result);
    }
}
