CREATE PROCEDURE [dbo].[usp_MachineBreakdowns_GetReportData]
    @StartDate DATE,
    @EndDate DATE,
    @MachineName NVARCHAR(255) = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT 
        mb.ReportedDateTime,
        mb.MachineName,
        mb.BreakdownReason,
        reporter.FullName AS ReportedByUsername,
        mb.IsResolved,
        mb.ResolvedDateTime,
        resolver.FullName AS ResolvedByUsername,
        mb.ResolutionNotes,
        mb.DowntimeMinutes
    FROM [dbo].[MachineBreakdowns] mb
    LEFT JOIN [dbo].[Users] reporter ON mb.ReportedByUserID = reporter.UserID
    LEFT JOIN [dbo].[Users] resolver ON mb.ResolvedByUserID = resolver.UserID
    WHERE mb.IsActive = 1 
      AND mb.ReportedDateTime >= @StartDate 
      AND mb.ReportedDateTime < @InclusiveEndDate
      AND (@MachineName IS NULL OR mb.MachineName = @MachineName)
      AND (@Status IS NULL 
           OR (@Status = 'Pending' AND mb.IsResolved = 0) 
           OR (@Status = 'Resolved' AND mb.IsResolved = 1))
    ORDER BY mb.ReportedDateTime DESC;
END
