CREATE PROCEDURE [dbo].[usp_MachineBreakdowns_MarkAsResolved]
    @BreakdownId INT,
    @UserId INT,
    @ResolutionNotes NVARCHAR(MAX)
AS
BEGIN
        SET XACT_ABORT ON;
    UPDATE [dbo].[MachineBreakdowns] 
    SET IsResolved = 1, 
        ResolvedByUserID = @UserId, 
        ResolvedDateTime = GETUTCDATE(),
        ResolutionNotes = @ResolutionNotes,
        DowntimeMinutes = DATEDIFF(minute, ReportedDateTime, GETUTCDATE())
    WHERE BreakdownID = @BreakdownId 
      AND IsResolved = 0;
END
