using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Data;

public class HandoverRepository(IDbConnectionFactory connectionFactory) : IHandoverRepository
{
    public async Task<Handover> CreateAsync(Handover handover)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO Handovers (HandoverNotes, GivenByUserID)
                           OUTPUT INSERTED.*
                           VALUES (@HandoverNotes, @GivenByUserID);
                           """;
        return await connection.QuerySingleAsync<Handover>(sql, handover);
    }

    public async Task<IEnumerable<Handover>> GetPendingAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM Handovers WHERE IsReceived = 0 ORDER BY GivenDateTime DESC";
        return await connection.QueryAsync<Handover>(sql);
    }

    public async Task<Handover?> GetByIdAsync(int handoverId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM Handovers WHERE HandoverID = @HandoverId";
        return await connection.QuerySingleOrDefaultAsync<Handover>(sql, new { HandoverId = handoverId });
    }

    public async Task<bool> MarkAsReceivedAsync(int handoverId, int userId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           UPDATE Handovers 
                           SET IsReceived = 1, ReceivedByUserID = @UserId, ReceivedDateTime = GETDATE() 
                           WHERE HandoverID = @HandoverId AND IsReceived = 0;
                           """;
        var rowsAffected = await connection.ExecuteAsync(sql, new { HandoverId = handoverId, UserId = userId });
        return rowsAffected > 0;
    }
}