CREATE PROCEDURE [dbo].[usp_MediaSterilityChecks_GetByDateRange]
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT SterilityCheckID, MediaName, MediaLotNumber, MediaQuantity, Result37C, Result25C, 
           OverallStatus, Comments, CheckDateTime, PerformedByUserID, IsActive, 
           DeactivationReason, DeactivatedByUserID, DeactivationDateTime
    FROM [dbo].[MediaSterilityChecks]
    WHERE IsActive = 1 
      AND CheckDateTime >= @StartDate 
      AND CheckDateTime < @InclusiveEndDate
    ORDER BY CheckDateTime DESC;
END
