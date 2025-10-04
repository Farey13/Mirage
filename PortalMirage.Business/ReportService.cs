using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class ReportService(IMachineBreakdownRepository machineBreakdownRepository) : IReportService
{
    public async Task<IEnumerable<MachineBreakdownReportDto>> GetMachineBreakdownReportAsync(DateTime startDate, DateTime endDate, string? machineName, string? status)
    {
        return await machineBreakdownRepository.GetReportDataAsync(startDate, endDate, machineName, status);
    }
}