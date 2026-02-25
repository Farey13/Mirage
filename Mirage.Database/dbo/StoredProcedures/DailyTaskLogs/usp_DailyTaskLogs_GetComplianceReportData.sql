CREATE PROCEDURE [dbo].[usp_DailyTaskLogs_GetComplianceReportData]
    @StartDate DATE,
    @EndDate DATE,
    @ShiftId INT = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT 
        dtl.LogDate,
        t.TaskName,
        s.ShiftName,
        dtl.Status,
        dtl.Comments,
        u.FullName AS CompletedByUserName,
        dtl.CompletedDateTime
    FROM [dbo].[DailyTaskLogs] dtl
    INNER JOIN [dbo].[Tasks] t ON dtl.TaskID = t.TaskID
    LEFT JOIN [dbo].[Shifts] s ON t.ShiftID = s.ShiftID
    LEFT JOIN [dbo].[Users] u ON dtl.CompletedByUserID = u.UserID
    WHERE dtl.LogDate >= @StartDate AND dtl.LogDate < @InclusiveEndDate
      AND (@ShiftId IS NULL OR t.ShiftID = @ShiftId)
      AND (@Status IS NULL OR @Status = 'All' OR dtl.Status = @Status)
    ORDER BY dtl.LogDate, s.StartTime;
END
