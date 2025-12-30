using PortalMirage.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions
{
    public interface IReportService
    {
        // ==========================================
        // DATA RETRIEVAL METHODS (Complete)
        // ==========================================
        Task<IEnumerable<MachineBreakdownReportDto>> GetMachineBreakdownReportAsync(DateTime startDate, DateTime endDate, string? machineName, string? status);
        Task<IEnumerable<HandoverReportDto>> GetHandoverReportAsync(DateTime startDate, DateTime endDate, string? shift, string? priority, string? status);
        Task<IEnumerable<KitValidationReportDto>> GetKitValidationReportAsync(DateTime startDate, DateTime endDate, string? kitName, string? status);
        Task<IEnumerable<RepeatSampleReportDto>> GetRepeatSampleReportAsync(DateTime startDate, DateTime endDate, string? reason, string? department);
        Task<DailyTaskComplianceReportDto> GetDailyTaskComplianceReportAsync(DateTime startDate, DateTime endDate, int? shiftId, string? status);
        Task<IEnumerable<MediaSterilityReportDto>> GetMediaSterilityReportAsync(DateTime startDate, DateTime endDate, string? mediaName, string? status);
        Task<IEnumerable<SampleStorageReportDto>> GetSampleStorageReportAsync(DateTime startDate, DateTime endDate, string? testName, string? status);
        Task<IEnumerable<CalibrationReportDto>> GetCalibrationReportAsync(DateTime startDate, DateTime endDate, string? testName, string? qcResult);

        // ==========================================
        // PDF GENERATION METHODS (Fixed)
        // ==========================================
        // Existing
        byte[] GenerateMachineBreakdownReport(List<MachineBreakdownReportDto> data);
        byte[] GenerateHandoverReport(List<HandoverReportDto> data);
        byte[] GenerateRepeatSampleReport(List<RepeatSampleReportDto> data);
        byte[] GenerateDailyTaskReport(List<DailyTaskLogDto> logs, DateTime startDate, DateTime endDate);

        // --- MISSING METHODS ADDED BELOW ---
        byte[] GenerateKitValidationReport(List<KitValidationReportDto> data);
        byte[] GenerateMediaSterilityReport(List<MediaSterilityReportDto> data);
        byte[] GenerateSampleStorageReport(List<SampleStorageReportDto> data);
        byte[] GenerateCalibrationReport(List<CalibrationReportDto> data);
    }
}