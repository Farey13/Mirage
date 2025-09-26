using PortalMirage.Core.Models;

namespace PortalMirage.Data.Abstractions;

public interface ICalibrationLogRepository
{
    Task<CalibrationLog> CreateAsync(CalibrationLog calibrationLog);
    Task<IEnumerable<CalibrationLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    Task<bool> DeactivateAsync(int logId, int userId, string reason);

    

}