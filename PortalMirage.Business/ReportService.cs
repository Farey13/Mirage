using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class ReportService(
    IMachineBreakdownRepository machineBreakdownRepository,
    IHandoverRepository handoverRepository,
    IKitValidationRepository kitValidationRepository) // 1. INJECT REPO
    : IReportService
{
    public async Task<IEnumerable<MachineBreakdownReportDto>> GetMachineBreakdownReportAsync(DateTime startDate, DateTime endDate, string? machineName, string? status)
    {
        return await machineBreakdownRepository.GetReportDataAsync(startDate, endDate, machineName, status);
    }

    public async Task<IEnumerable<HandoverReportDto>> GetHandoverReportAsync(DateTime startDate, DateTime endDate, string? shift, string? priority, string? status)
    {
        return await handoverRepository.GetReportDataAsync(startDate, endDate, shift, priority, status);
    }

    // 2. ADD NEW METHOD
    public async Task<IEnumerable<KitValidationReportDto>> GetKitValidationReportAsync(DateTime startDate, DateTime endDate, string? kitName, string? status)
    {
        return await kitValidationRepository.GetReportDataAsync(startDate, endDate, kitName, status);
    }
}