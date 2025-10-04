using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Data.Abstractions;

public interface IHandoverRepository
{
    Task<Handover> CreateAsync(Handover handover);
    Task<IEnumerable<Handover>> GetPendingAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Handover>> GetCompletedAsync(DateTime startDate, DateTime endDate);
    Task<Handover?> GetByIdAsync(int handoverId);
    Task<bool> MarkAsReceivedAsync(int handoverId, int userId);
    Task<bool> DeactivateAsync(int handoverId, int userId, string reason);

    Task<int> GetPendingCountAsync(); // ADD THIS LINE

    Task<IEnumerable<HandoverReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? shift, string? priority, string? status); // ADD THIS LINE


}