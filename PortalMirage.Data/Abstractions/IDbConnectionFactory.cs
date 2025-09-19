using System.Data;

namespace PortalMirage.Data.Abstractions;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}