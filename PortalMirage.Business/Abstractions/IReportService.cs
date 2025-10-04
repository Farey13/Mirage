using PortalMirage.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions;

public interface IReportService
{
    Task<IEnumerable<MachineBreakdownReportDto>> GetMachineBreakdownReportAsync(DateTime startDate, DateTime endDate, string? machineName, string? status);

    Task<IEnumerable<KitValidationReportDto>> GetKitValidationReportAsync(DateTime startDate, DateTime endDate, string? kitName, string? status); // ADD THIS
    Task<IEnumerable<HandoverReportDto>> GetHandoverReportAsync(DateTime startDate, DateTime endDate, string? shift, string? priority, string? status); // ADD THIS LINE
}