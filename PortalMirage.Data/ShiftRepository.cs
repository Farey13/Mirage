using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class ShiftRepository(IDbConnectionFactory connectionFactory) : IShiftRepository
{
    // === 1. READ ALL ===
    public async Task<IEnumerable<Shift>> GetAllAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM Shifts WHERE IsActive = 1";

        // Use dynamic to read raw values
        var rows = await connection.QueryAsync<dynamic>(sql);
        var shifts = new List<Shift>();

        foreach (var row in rows)
        {
            // Manual mapping - simple and explicit
            shifts.Add(MapRowToShift(row));
        }

        return shifts;
    }

    // === 2. READ BY ID ===
    public async Task<Shift?> GetByIdAsync(int shiftId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM Shifts WHERE ShiftID = @ShiftId";

        var row = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { ShiftId = shiftId });

        if (row == null) return null;

        return MapRowToShift(row);
    }

    // === 3. CREATE ===
    public async Task<Shift> CreateAsync(Shift shift)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
            INSERT INTO Shifts (ShiftName, StartTime, EndTime, GracePeriodHours, IsActive)
            OUTPUT INSERTED.ShiftID
            VALUES (@ShiftName, @StartTime, @EndTime, @GracePeriodHours, @IsActive);
            """;

        // Explicitly convert TimeOnly -> TimeSpan for SQL
        var parameters = new
        {
            shift.ShiftName,
            StartTime = shift.StartTime.ToTimeSpan(),
            EndTime = shift.EndTime.ToTimeSpan(),
            shift.GracePeriodHours,
            shift.IsActive
        };

        var newId = await connection.ExecuteScalarAsync<int>(sql, parameters);
        return shift with { ShiftID = newId };
    }

    // === 4. UPDATE ===
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
            WHERE ShiftID = @ShiftID;
            """;

        var parameters = new
        {
            shift.ShiftID,
            shift.ShiftName,
            StartTime = shift.StartTime.ToTimeSpan(),
            EndTime = shift.EndTime.ToTimeSpan(),
            shift.GracePeriodHours,
            shift.IsActive
        };

        await connection.ExecuteAsync(sql, parameters);
        return shift;
    }

    // === 5. DEACTIVATE ===
    public async System.Threading.Tasks.Task DeactivateAsync(int shiftId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "UPDATE Shifts SET IsActive = 0 WHERE ShiftID = @ShiftId";
        await connection.ExecuteAsync(sql, new { ShiftId = shiftId });
    }

    // === HELPER: Centralized Mapping Logic ===
    private Shift MapRowToShift(dynamic row)
    {
        return new Shift
        {
            ShiftID = (int)row.ShiftID,
            ShiftName = (string)row.ShiftName,
            // Safe conversion logic
            StartTime = ParseTime(row.StartTime),
            EndTime = ParseTime(row.EndTime),
            GracePeriodHours = row.GracePeriodHours != null ? (int)row.GracePeriodHours : 2,
            IsActive = row.IsActive != null && Convert.ToBoolean(row.IsActive)
        };
    }

    // === HELPER: Safe Time Conversion ===
    private TimeOnly ParseTime(object value)
    {
        if (value == null) return TimeOnly.MinValue;

        // Standard SQL Time maps to TimeSpan
        if (value is TimeSpan ts)
            return TimeOnly.FromTimeSpan(ts);

        // Legacy SQL DateTime maps to DateTime
        if (value is DateTime dt)
            return TimeOnly.FromDateTime(dt);

        // Fallback for strings
        if (TimeSpan.TryParse(value.ToString(), out var parsedTs))
            return TimeOnly.FromTimeSpan(parsedTs);

        return TimeOnly.MinValue;
    }
}