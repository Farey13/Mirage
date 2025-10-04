using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using Refit;
using System;
using System.Collections.Generic;

namespace Mirage.UI.Services;

public interface IPortalMirageApi
{
    // === Login ===
    [Post("/api/auth/login")]
    System.Threading.Tasks.Task<LoginResponse> LoginAsync([Body] LoginRequest loginRequest);

    // === Calibration Log ===
    [Post("/api/calibrationlogs")]
    System.Threading.Tasks.Task<CalibrationLogResponse> CreateCalibrationLogAsync([Header("Authorization")] string token, [Body] CreateCalibrationLogRequest request);

    [Get("/api/calibrationlogs")]
    System.Threading.Tasks.Task<List<CalibrationLogResponse>> GetCalibrationLogsAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/calibrationlogs/{id}/deactivate")]
    System.Threading.Tasks.Task DeactivateCalibrationLogAsync([Header("Authorization")] string token, int id, [Body] DeactivateCalibrationLogRequest request);

    // === Kit Validation ===
    [Post("/api/kitvalidations")]
    System.Threading.Tasks.Task<KitValidationResponse> CreateKitValidationAsync([Header("Authorization")] string token, [Body] CreateKitValidationRequest request);

    [Get("/api/kitvalidations")]
    System.Threading.Tasks.Task<List<KitValidationResponse>> GetKitValidationsAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/kitvalidations/{id}/deactivate")]
    System.Threading.Tasks.Task DeactivateKitValidationAsync([Header("Authorization")] string token, int id, [Body] DeactivateKitValidationRequest request);

    // === Sample Storage ===
    [Post("/api/samplestorage")]
    System.Threading.Tasks.Task<SampleStorageResponse> CreateSampleAsync([Header("Authorization")] string token, [Body] CreateSampleStorageRequest request);

    [Get("/api/samplestorage/pending")]
    System.Threading.Tasks.Task<List<SampleStorageResponse>> GetPendingSamplesAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Get("/api/samplestorage/completed")]
    System.Threading.Tasks.Task<List<SampleStorageResponse>> GetCompletedSamplesAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/samplestorage/{id}/done")]
    System.Threading.Tasks.Task MarkSampleAsDoneAsync([Header("Authorization")] string token, int id);

    [Put("/api/samplestorage/{id}/deactivate")]
    System.Threading.Tasks.Task DeactivateSampleAsync([Header("Authorization")] string token, int id, [Body] DeactivateSampleStorageRequest request);

    // === Handover Book ===
    [Post("/api/handovers")]
    System.Threading.Tasks.Task<HandoverResponse> CreateHandoverAsync([Header("Authorization")] string token, [Body] CreateHandoverRequest request);

    [Get("/api/handovers/pending")]
    System.Threading.Tasks.Task<List<HandoverResponse>> GetPendingHandoversAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Get("/api/handovers/completed")]
    System.Threading.Tasks.Task<List<HandoverResponse>> GetCompletedHandoversAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/handovers/{id}/receive")]
    System.Threading.Tasks.Task MarkHandoverAsReceivedAsync([Header("Authorization")] string token, int id);

    [Put("/api/handovers/{id}/deactivate")]
    System.Threading.Tasks.Task DeactivateHandoverAsync([Header("Authorization")] string token, int id, [Body] DeactivateHandoverRequest request);

    // === Machine Breakdown ===
    [Post("/api/machinebreakdowns")]
    System.Threading.Tasks.Task<MachineBreakdownResponse> CreateBreakdownAsync([Header("Authorization")] string token, [Body] CreateMachineBreakdownRequest request);

    [Get("/api/machinebreakdowns/pending")]
    System.Threading.Tasks.Task<List<MachineBreakdownResponse>> GetPendingBreakdownsAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Get("/api/machinebreakdowns/resolved")]
    System.Threading.Tasks.Task<List<MachineBreakdownResponse>> GetResolvedBreakdownsAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/machinebreakdowns/{id}/resolve")]
    System.Threading.Tasks.Task MarkBreakdownAsResolvedAsync([Header("Authorization")] string token, int id, [Body] ResolveBreakdownRequest request);

    [Put("/api/machinebreakdowns/{id}/deactivate")]
    System.Threading.Tasks.Task DeactivateBreakdownAsync([Header("Authorization")] string token, int id, [Body] DeactivateMachineBreakdownRequest request);

    // --- Media Sterility ---
    [Post("/api/mediasterilitychecks")]
    System.Threading.Tasks.Task<MediaSterilityCheckResponse> CreateSterilityCheckAsync([Header("Authorization")] string token, [Body] CreateMediaSterilityCheckRequest request);

    [Get("/api/mediasterilitychecks")]
    System.Threading.Tasks.Task<List<MediaSterilityCheckResponse>> GetSterilityChecksAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/mediasterilitychecks/{id}/deactivate")]
    System.Threading.Tasks.Task DeactivateSterilityCheckAsync([Header("Authorization")] string token, int id, [Body] DeactivateMediaSterilityCheckRequest request);

    // --- Repeat Sample Book ---
    [Post("/api/repeatsamples")]
    System.Threading.Tasks.Task<RepeatSampleResponse> CreateRepeatSampleAsync([Header("Authorization")] string token, [Body] CreateRepeatSampleRequest request);

    [Get("/api/repeatsamples")]
    System.Threading.Tasks.Task<List<RepeatSampleResponse>> GetRepeatSamplesAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/repeatsamples/{id}/deactivate")]
    System.Threading.Tasks.Task DeactivateRepeatSampleAsync([Header("Authorization")] string token, int id, [Body] DeactivateRepeatSampleRequest request);

    // --- Daily Task Log ---
    [Get("/api/dailytasklogs")]
    System.Threading.Tasks.Task<List<TaskLogDetailDto>> GetTasksForDateAsync([Header("Authorization")] string token, [Query] DateTime date);

    [Put("/api/dailytasklogs/{id}/status")]
    System.Threading.Tasks.Task<TaskLogDetailDto> UpdateTaskStatusAsync([Header("Authorization")] string token, long id, [Body] UpdateTaskStatusRequest request);

    [Put("/api/dailytasklogs/{id}/extend")]
    System.Threading.Tasks.Task<DailyTaskLog> ExtendTaskDeadlineAsync([Header("Authorization")] string token, long id, [Body] ExtendTaskDeadlineRequest request);

    // --- Admin Panel ---
    [Get("/api/admin/users")]
    System.Threading.Tasks.Task<List<UserResponse>> GetAllUsersAsync([Header("Authorization")] string token);

    [Get("/api/admin/roles")]
    System.Threading.Tasks.Task<List<RoleResponse>> GetAllRolesAsync([Header("Authorization")] string token);

    [Get("/api/shifts")]
    System.Threading.Tasks.Task<List<ShiftResponse>> GetAllShiftsAsync([Header("Authorization")] string token);

    [Post("/api/admin/assign-role")]
    System.Threading.Tasks.Task AssignRoleAsync([Header("Authorization")] string token, [Body] AssignRoleRequest request);

    [Post("/api/admin/remove-role")]
    System.Threading.Tasks.Task RemoveRoleFromUserAsync([Header("Authorization")] string token, [Body] AssignRoleRequest request);

    [Get("/api/admin/users/{username}/roles")]
    System.Threading.Tasks.Task<List<RoleResponse>> GetRolesForUserAsync([Header("Authorization")] string token, string username);

    [Post("/api/shifts")]
    System.Threading.Tasks.Task<ShiftResponse> CreateShiftAsync([Header("Authorization")] string token, [Body] CreateShiftRequest request);



    [Put("/api/shifts/{id}")]
    System.Threading.Tasks.Task<ShiftResponse> UpdateShiftAsync([Header("Authorization")] string token, int id, [Body] UpdateShiftRequest request);

    [Delete("/api/shifts/{id}")]
    System.Threading.Tasks.Task DeactivateShiftAsync([Header("Authorization")] string token, int id);

    // --- Admin Master Lists ---
    [Get("/api/admin/lists")]
    System.Threading.Tasks.Task<List<AdminListItemDto>> GetAllListItemsAsync([Header("Authorization")] string token);

    [Get("/api/admin/lists/types")]
    System.Threading.Tasks.Task<List<string>> GetListTypesAsync([Header("Authorization")] string token);

    // ADD THIS NEW METHOD
    [Get("/api/admin/lists/{listType}")]
    System.Threading.Tasks.Task<List<AdminListItemDto>> GetListItemsByTypeAsync([Header("Authorization")] string token, string listType);


    [Post("/api/admin/lists")]
    System.Threading.Tasks.Task<AdminListItemDto> CreateListItemAsync([Header("Authorization")] string token, [Body] CreateAdminListItemRequest request);

    [Put("/api/admin/lists/{id}")]
    System.Threading.Tasks.Task<AdminListItemDto> UpdateListItemAsync([Header("Authorization")] string token, int id, [Body] UpdateAdminListItemRequest request);

    // === Audit Log ===
    [Get("/api/auditlogs")]
    System.Threading.Tasks.Task<List<AuditLogDto>> GetAuditLogsAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    // === Admin User Management ===
    [Post("/api/admin/users/create")]
    System.Threading.Tasks.Task<UserResponse> CreateUserAsync([Header("Authorization")] string token, [Body] CreateUserRequest request);

    [Post("/api/admin/roles")] // ADD THIS
    System.Threading.Tasks.Task<RoleResponse> CreateRoleAsync([Header("Authorization")] string token, [Body] CreateRoleRequest request);

    [Post("/api/admin/users/reset-password")] // ADD THIS
    System.Threading.Tasks.Task ResetPasswordAsync([Header("Authorization")] string token, [Body] ResetPasswordRequest request);

    // === Dashboard ===
    [Get("/api/dashboard/summary")] // ADD THIS ENDPOINT
    System.Threading.Tasks.Task<DashboardSummaryDto> GetDashboardSummaryAsync([Header("Authorization")] string token);

    // === Reports ===
    [Get("/api/reports/machine-breakdowns")] // ADD THIS
    System.Threading.Tasks.Task<List<MachineBreakdownReportDto>> GetMachineBreakdownReportAsync(
        [Header("Authorization")] string token,
        [Query] DateTime startDate,
        [Query] DateTime endDate,
        [Query] string? machineName,
        [Query] string? status);
}
