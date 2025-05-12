using System.Data.Common;

namespace JANORM.Client.services;

public interface IDBFactory
{
    DbConnection CreateConnection();
}
