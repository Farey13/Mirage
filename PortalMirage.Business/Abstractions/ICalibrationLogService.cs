using PortalMirage.Core.Models;

namespace PortalMirage.Business.Abstractions;

public interface ICalibrationLogService
{
    Task<CalibrationLog> CreateAsync(CalibrationLog calibrationLog);
    Task<IEnumerable<CalibrationLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}