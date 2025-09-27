using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mirage.UI.Services;

public interface IPortalMirageApi
{
    // === Login ===
    [Post("/api/auth/login")]
    Task<LoginResponse> LoginAsync([Body] LoginRequest loginRequest);

    // === Calibration Log ===
    [Post("/api/calibrationlogs")]
    Task<CalibrationLogResponse> CreateCalibrationLogAsync([Header("Authorization")] string token, [Body] CreateCalibrationLogRequest request);

    [Get("/api/calibrationlogs")]
    Task<List<CalibrationLogResponse>> GetCalibrationLogsAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/calibrationlogs/{id}/deactivate")]
    Task DeactivateCalibrationLogAsync([Header("Authorization")] string token, int id, [Body] DeactivateCalibrationLogRequest request);

    // === Kit Validation ===
    [Post("/api/kitvalidations")]
    Task<KitValidationResponse> CreateKitValidationAsync([Header("Authorization")] string token, [Body] CreateKitValidationRequest request);

    [Get("/api/kitvalidations")]
    Task<List<KitValidationResponse>> GetKitValidationsAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/kitvalidations/{id}/deactivate")]
    Task DeactivateKitValidationAsync([Header("Authorization")] string token, int id, [Body] DeactivateKitValidationRequest request);

    // === Sample Storage ===
    [Post("/api/samplestorage")]
    Task<SampleStorageResponse> CreateSampleAsync([Header("Authorization")] string token, [Body] CreateSampleStorageRequest request);

    [Get("/api/samplestorage/pending")]
    Task<List<SampleStorageResponse>> GetPendingSamplesAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Get("/api/samplestorage/completed")]
    Task<List<SampleStorageResponse>> GetCompletedSamplesAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/samplestorage/{id}/done")]
    Task MarkSampleAsDoneAsync([Header("Authorization")] string token, int id);

    [Put("/api/samplestorage/{id}/deactivate")]
    Task DeactivateSampleAsync([Header("Authorization")] string token, int id, [Body] DeactivateSampleStorageRequest request);

    // === Handover Book ===
    [Post("/api/handovers")]
    Task<HandoverResponse> CreateHandoverAsync([Header("Authorization")] string token, [Body] CreateHandoverRequest request);

    [Get("/api/handovers/pending")]
    Task<List<HandoverResponse>> GetPendingHandoversAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Get("/api/handovers/completed")]
    Task<List<HandoverResponse>> GetCompletedHandoversAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/handovers/{id}/receive")]
    Task MarkHandoverAsReceivedAsync([Header("Authorization")] string token, int id);

    [Put("/api/handovers/{id}/deactivate")]
    Task DeactivateHandoverAsync([Header("Authorization")] string token, int id, [Body] DeactivateHandoverRequest request);

    // === Machine Breakdown ===
    [Post("/api/machinebreakdowns")]
    Task<MachineBreakdownResponse> CreateBreakdownAsync([Header("Authorization")] string token, [Body] CreateMachineBreakdownRequest request);

    [Get("/api/machinebreakdowns/pending")]
    Task<List<MachineBreakdownResponse>> GetPendingBreakdownsAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Get("/api/machinebreakdowns/resolved")]
    Task<List<MachineBreakdownResponse>> GetResolvedBreakdownsAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/machinebreakdowns/{id}/resolve")]
    Task MarkBreakdownAsResolvedAsync([Header("Authorization")] string token, int id, [Body] ResolveBreakdownRequest request);

    [Put("/api/machinebreakdowns/{id}/deactivate")]
    Task DeactivateBreakdownAsync([Header("Authorization")] string token, int id, [Body] DeactivateMachineBreakdownRequest request);

    // --- Media Sterility ---
    [Post("/api/mediasterilitychecks")]
    Task<MediaSterilityCheckResponse> CreateSterilityCheckAsync([Header("Authorization")] string token, [Body] CreateMediaSterilityCheckRequest request);

    [Get("/api/mediasterilitychecks")]
    Task<List<MediaSterilityCheckResponse>> GetSterilityChecksAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/mediasterilitychecks/{id}/deactivate")]
    Task DeactivateSterilityCheckAsync([Header("Authorization")] string token, int id, [Body] DeactivateMediaSterilityCheckRequest request);

    // --- Repeat Sample Book ---
    [Post("/api/repeatsamples")]
    Task<RepeatSampleResponse> CreateRepeatSampleAsync([Header("Authorization")] string token, [Body] CreateRepeatSampleRequest request);

    [Get("/api/repeatsamples")]
    Task<List<RepeatSampleResponse>> GetRepeatSamplesAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/repeatsamples/{id}/deactivate")]
    Task DeactivateRepeatSampleAsync([Header("Authorization")] string token, int id, [Body] DeactivateRepeatSampleRequest request);
}
