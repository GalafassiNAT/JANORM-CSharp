using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace JANORM.Core.services.Implementation;

public class SqliteConnectionFactory : IDBFactory
{   
    private readonly string _conectionString;

    public SqliteConnectionFactory(string conectionString) 
    {
        _conectionString = conectionString;
    }

    public DbConnection CreateConnection() 
    {
        var connection = new SqliteConnection(_conectionString);
        connection.Open();
        return connection;
    }
}
