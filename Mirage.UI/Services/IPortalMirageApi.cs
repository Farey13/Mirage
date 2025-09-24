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
}