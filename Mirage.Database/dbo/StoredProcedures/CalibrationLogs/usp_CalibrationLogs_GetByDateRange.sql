CREATE PROCEDURE [dbo].[usp_CalibrationLogs_GetByDateRange]
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT CalibrationID, TestName, QcResult, Reason, CalibrationDateTime, PerformedByUserID, 
           IsActive, DeactivationReason, DeactivatedByUserID, DeactivationDateTime
    FROM [dbo].[CalibrationLogs]
    WHERE IsActive = 1 
      AND CalibrationDateTime >= @StartDate 
      AND CalibrationDateTime < @InclusiveEndDate
    ORDER BY CalibrationDateTime DESC;
END
