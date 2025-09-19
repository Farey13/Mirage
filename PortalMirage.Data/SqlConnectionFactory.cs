using System.Data;
using Microsoft.Data.SqlClient;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Data;

public class SqlConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }
}