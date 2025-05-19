using System.Data.Common;

namespace JANORM.Core.services;

public interface IDBFactory
{
    DbConnection CreateConnection();
}
