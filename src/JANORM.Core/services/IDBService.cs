using System.Data.Common;

namespace JANORM.Core.services;

public interface IDBService
{
    Task ExecuteNonQueryAsync(string sql, params DbParameter[] parameters);
    Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, params DbParameter[] parameters);
    Task<object?> ExecuteScalarAsync(string sql, params DbParameter[] parameters);
}
