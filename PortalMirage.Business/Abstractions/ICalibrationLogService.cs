using PortalMirage.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions;

public interface ICalibrationLogService
{
    Task<CalibrationLog> CreateAsync(CalibrationLog calibrationLog);
    Task<IEnumerable<CalibrationLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> DeactivateAsync(int logId, int userId, string reason); // Add this line
}