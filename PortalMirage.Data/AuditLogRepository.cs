using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
// We add "using Task = System.Threading.Tasks.Task;" as an alias
using Task = System.Threading.Tasks.Task;

namespace PortalMirage.Data;

public class AuditLogRepository(IDbConnectionFactory connectionFactory) : IAuditLogRepository
{
    // Now we can just use "Task" and the compiler knows what we mean
    public async Task CreateAsync(AuditLog logEntry)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO AuditLog (UserID, ActionType, ModuleName, RecordID, FieldName, OldValue, NewValue)
                           VALUES (@UserID, @ActionType, @ModuleName, @RecordID, @FieldName, @OldValue, @NewValue);
                           """;
        await connection.ExecuteAsync(sql, logEntry);
    }
}