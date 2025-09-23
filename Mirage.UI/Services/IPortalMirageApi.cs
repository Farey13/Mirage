using PortalMirage.Core.Dtos; // Add this using statement
using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mirage.UI.Services;

public interface IPortalMirageApi
{
    // Login
    [Post("/api/auth/login")]
    Task<LoginResponse> LoginAsync([Body] LoginRequest loginRequest);

    // Calibration Log
    [Post("/api/calibrationlogs")]
    Task<CalibrationLogResponse> CreateCalibrationLogAsync([Header("Authorization")] string token, [Body] CreateCalibrationLogRequest request);

    [Get("/api/calibrationlogs")]
    Task<List<CalibrationLogResponse>> GetCalibrationLogsAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);

    // Kit Validation
    [Post("/api/kitvalidations")]
    Task<KitValidationResponse> CreateKitValidationAsync([Header("Authorization")] string token, [Body] CreateKitValidationRequest request);

    [Get("/api/kitvalidations")]
    Task<List<KitValidationResponse>> GetKitValidationsAsync([Header("Authorization")] string token, [Query] DateTime startDate, [Query] DateTime endDate);
}