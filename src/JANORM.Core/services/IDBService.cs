using System.Data.Common;

namespace JANORM.Core.services;

public interface IDBService
{
    Task<int> ExecuteNonQueryAsync(string sql, params DbParameter[] parameters);
    Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, params DbParameter[] parameters);
}
