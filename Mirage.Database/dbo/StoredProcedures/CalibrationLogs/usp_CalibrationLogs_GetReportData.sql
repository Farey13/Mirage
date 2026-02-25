CREATE PROCEDURE [dbo].[usp_CalibrationLogs_GetReportData]
    @StartDate DATE,
    @EndDate DATE,
    @TestName NVARCHAR(255) = NULL,
    @QcResult NVARCHAR(50) = NULL
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT 
        c.CalibrationDateTime,
        c.TestName,
        c.QcResult,
        c.Reason,
        u.FullName AS PerformedByUsername
    FROM [dbo].[CalibrationLogs] c
    LEFT JOIN [dbo].[Users] u ON c.PerformedByUserID = u.UserID
    WHERE c.IsActive = 1 
      AND c.CalibrationDateTime >= @StartDate 
      AND c.CalibrationDateTime < @InclusiveEndDate
      AND (@TestName IS NULL OR @TestName = 'All' OR c.TestName = @TestName)
      AND (@QcResult IS NULL OR @QcResult = 'All' OR c.QcResult = @QcResult)
    ORDER BY c.CalibrationDateTime DESC;
END
