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
}