using PortalMirage.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions
{
    public interface IAuditLogService
    {
        /// <summary>
        /// Creates a new audit log entry in the database.
        /// </summary>
        Task LogAsync(int? userId, string actionType, string moduleName, string? recordId = null, string? fieldName = null, string? oldValue = null, string? newValue = null);

        /// <summary>
        /// Retrieves a collection of audit logs within a specified date range for display.
        /// </summary>
        Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}