using PortalMirage.Core.Models;

namespace PortalMirage.Data.Abstractions;

public interface ICalibrationLogRepository
{
    Task<CalibrationLog> CreateAsync(CalibrationLog calibrationLog);
    Task<IEnumerable<CalibrationLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}