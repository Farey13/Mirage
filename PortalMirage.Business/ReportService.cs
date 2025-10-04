using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class ReportService(
    IMachineBreakdownRepository machineBreakdownRepository,
    IHandoverRepository handoverRepository) // 1. INJECT THE NEW REPOSITORY
    : IReportService
{
    public async Task<IEnumerable<MachineBreakdownReportDto>> GetMachineBreakdownReportAsync(DateTime startDate, DateTime endDate, string? machineName, string? status)
    {
        return await machineBreakdownRepository.GetReportDataAsync(startDate, endDate, machineName, status);
    }

    // 2. ADD THE NEW METHOD IMPLEMENTATION
    public async Task<IEnumerable<HandoverReportDto>> GetHandoverReportAsync(DateTime startDate, DateTime endDate, string? shift, string? priority, string? status)
    {
        return await handoverRepository.GetReportDataAsync(startDate, endDate, shift, priority, status);
    }
}