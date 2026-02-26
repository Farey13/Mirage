CREATE PROCEDURE [dbo].[usp_MachineBreakdowns_Create]
    @MachineName NVARCHAR(255),
    @BreakdownReason NVARCHAR(MAX),
    @ReportedByUserID INT
AS
BEGIN
        SET XACT_ABORT ON;
    INSERT INTO [dbo].[MachineBreakdowns] (MachineName, BreakdownReason, ReportedByUserID)
    OUTPUT INSERTED.BreakdownID, INSERTED.MachineName, INSERTED.BreakdownReason, INSERTED.ReportedDateTime, 
           INSERTED.ReportedByUserID, INSERTED.IsResolved, INSERTED.ResolvedDateTime, INSERTED.ResolvedByUserID, 
           INSERTED.ResolutionNotes, INSERTED.DowntimeMinutes, INSERTED.IsActive, 
           INSERTED.DeactivationReason, INSERTED.DeactivatedByUserID, INSERTED.DeactivationDateTime
    VALUES (@MachineName, @BreakdownReason, @ReportedByUserID);
END
