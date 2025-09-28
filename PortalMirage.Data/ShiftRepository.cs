using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class ShiftRepository(IDbConnectionFactory connectionFactory) : IShiftRepository
{
    public async Task<IEnumerable<Shift>> GetAllAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM Shifts WHERE IsActive = 1";
        return await connection.QueryAsync<Shift>(sql);
    }

    public async Task<Shift?> GetByIdAsync(int shiftId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM Shifts WHERE ShiftID = @ShiftId";
        return await connection.QuerySingleOrDefaultAsync<Shift>(sql, new { ShiftId = shiftId });
    }

    public async Task<Shift> CreateAsync(Shift shift)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO Shifts (ShiftName, StartTime, EndTime, GracePeriodHours, IsActive)
                           OUTPUT INSERTED.*
                           VALUES (@ShiftName, @StartTime, @EndTime, @GracePeriodHours, @IsActive);
                           """;
        return await connection.QuerySingleAsync<Shift>(sql, shift);
    }

    public async Task<Shift> UpdateAsync(Shift shift)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           UPDATE Shifts
                           SET ShiftName = @ShiftName,
                               StartTime = @StartTime,
                               EndTime = @EndTime,
                               GracePeriodHours = @GracePeriodHours,
                               IsActive = @IsActive
                           OUTPUT INSERTED.*
                           WHERE ShiftID = @ShiftID;
                           """;
        return await connection.QuerySingleAsync<Shift>(sql, shift);
    }
}