CREATE PROCEDURE [dbo].[usp_MachineBreakdowns_GetPendingByDateRange]
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT BreakdownID, MachineName, BreakdownReason, ReportedDateTime, ReportedByUserID, IsResolved, 
           ResolvedDateTime, ResolvedByUserID, ResolutionNotes, DowntimeMinutes, IsActive, 
           DeactivationReason, DeactivatedByUserID, DeactivationDateTime
    FROM [dbo].[MachineBreakdowns] 
    WHERE IsResolved = 0 
      AND IsActive = 1 
      AND ReportedDateTime >= @StartDate 
      AND ReportedDateTime < @InclusiveEndDate
    ORDER BY ReportedDateTime DESC;
END
