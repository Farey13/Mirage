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
    public async Task<IEnumerable<Shift>> GetAllAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var rows = await connection.QueryAsync<dynamic>(
            "usp_Shifts_GetAll",
            commandType: CommandType.StoredProcedure);
        
        var shifts = new List<Shift>();
        foreach (var row in rows)
        {
            shifts.Add(MapRowToShift(row));
        }
        return shifts;
    }

    public async Task<Shift?> GetByIdAsync(int shiftId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var row = await connection.QuerySingleOrDefaultAsync<dynamic>(
            "usp_Shifts_GetById",
            new { ShiftId = shiftId },
            commandType: CommandType.StoredProcedure);

        if (row == null) return null;
        return MapRowToShift(row);
    }

    public async Task<Shift> CreateAsync(Shift shift)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var newId = await connection.ExecuteScalarAsync<int>(
            "usp_Shifts_Create",
            new { ShiftName = shift.ShiftName, StartTime = shift.StartTime.ToTimeSpan(), EndTime = shift.EndTime.ToTimeSpan(), GracePeriodHours = shift.GracePeriodHours, IsActive = shift.IsActive },
            commandType: CommandType.StoredProcedure);
        return shift with { ShiftID = newId };
    }

    public async Task<Shift> UpdateAsync(Shift shift)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(
            "usp_Shifts_Update",
            new { ShiftID = shift.ShiftID, ShiftName = shift.ShiftName, StartTime = shift.StartTime.ToTimeSpan(), EndTime = shift.EndTime.ToTimeSpan(), GracePeriodHours = shift.GracePeriodHours, IsActive = shift.IsActive },
            commandType: CommandType.StoredProcedure);
        return shift;
    }

    public async System.Threading.Tasks.Task DeactivateAsync(int shiftId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(
            "usp_Shifts_Deactivate",
            new { ShiftId = shiftId },
            commandType: CommandType.StoredProcedure);
    }

    private Shift MapRowToShift(dynamic row)
    {
        return new Shift
        {
            ShiftID = (int)row.ShiftID,
            ShiftName = (string)row.ShiftName,
            StartTime = ParseTime(row.StartTime),
            EndTime = ParseTime(row.EndTime),
            GracePeriodHours = row.GracePeriodHours != null ? (int)row.GracePeriodHours : 2,
            IsActive = row.IsActive != null && Convert.ToBoolean(row.IsActive)
        };
    }

    private TimeOnly ParseTime(object value)
    {
        if (value == null) return TimeOnly.MinValue;
        if (value is TimeSpan ts) return TimeOnly.FromTimeSpan(ts);
        if (value is DateTime dt) return TimeOnly.FromDateTime(dt);
        if (TimeSpan.TryParse(value.ToString(), out var parsedTs)) return TimeOnly.FromTimeSpan(parsedTs);
        return TimeOnly.MinValue;
    }
}
