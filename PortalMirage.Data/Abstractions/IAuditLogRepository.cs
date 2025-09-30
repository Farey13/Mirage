using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models; // Add this using statement
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Data.Abstractions;

public interface IAuditLogRepository
{
    // This is the new method for retrieving logs
    Task<IEnumerable<AuditLogDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    // This is the missing method that the AuditLogService needs
    System.Threading.Tasks.Task CreateAsync(AuditLog logEntry);
}