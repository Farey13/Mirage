CREATE PROCEDURE [dbo].[usp_MachineBreakdowns_GetById]
    @BreakdownId INT
AS
BEGIN
    
    SELECT BreakdownID, MachineName, BreakdownReason, ReportedDateTime, ReportedByUserID, IsResolved, 
           ResolvedDateTime, ResolvedByUserID, ResolutionNotes, DowntimeMinutes, IsActive, 
           DeactivationReason, DeactivatedByUserID, DeactivationDateTime
    FROM [dbo].[MachineBreakdowns] 
    WHERE BreakdownID = @BreakdownId 
      AND IsActive = 1;
END
