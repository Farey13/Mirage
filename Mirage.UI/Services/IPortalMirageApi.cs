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

    // === Kit Validation ===
    [Post("/api/kitvalidations")]
    Task<KitValidationResponse> CreateKitValidationAsync([Header("Authorization")] string token, [Body] CreateKitValidationRequest request);

    [Get("/api/kitvalidations")]
    Task<List<KitValidationResponse>> GetKitValidationsAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    // === Sample Storage ===
    [Post("/api/samplestorage")]
    Task<SampleStorageResponse> CreateSampleAsync([Header("Authorization")] string token, [Body] CreateSampleStorageRequest request);

    [Get("/api/samplestorage/pending")]
    Task<List<SampleStorageResponse>> GetPendingSamplesAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/samplestorage/{id}/done")]
    Task MarkSampleAsDoneAsync([Header("Authorization")] string token, int id);

    // ... inside the IPortalMirageApi interface ...

    [Get("/api/samplestorage/completed")]
    Task<List<SampleStorageResponse>> GetCompletedSamplesAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Delete("/api/samplestorage/{id}")]
    Task DeactivateSampleAsync([Header("Authorization")] string token, int id);

    // ... (keep all existing methods) ...

    // --- Handover Book ---
    [Post("/api/handovers")]
    Task<HandoverResponse> CreateHandoverAsync([Header("Authorization")] string token, [Body] CreateHandoverRequest request);

    [Get("/api/handovers/pending")]
    Task<List<HandoverResponse>> GetPendingHandoversAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Get("/api/handovers/completed")]
    Task<List<HandoverResponse>> GetCompletedHandoversAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/handovers/{id}/receive")]
    Task MarkHandoverAsReceivedAsync([Header("Authorization")] string token, int id);


    // --- Machine Breakdown ---
    [Post("/api/machinebreakdowns")]
    Task<MachineBreakdownResponse> CreateBreakdownAsync([Header("Authorization")] string token, [Body] CreateMachineBreakdownRequest request);

    [Get("/api/machinebreakdowns/pending")]
    Task<List<MachineBreakdownResponse>> GetPendingBreakdownsAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Get("/api/machinebreakdowns/resolved")]
    Task<List<MachineBreakdownResponse>> GetResolvedBreakdownsAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    [Put("/api/machinebreakdowns/{id}/resolve")]
    Task MarkBreakdownAsResolvedAsync([Header("Authorization")] string token, int id, [Body] ResolveBreakdownRequest request);
}