CREATE PROCEDURE [dbo].[usp_RepeatSampleLog_GetReportData]
    @StartDate DATE,
    @EndDate DATE,
    @Reason NVARCHAR(MAX) = NULL,
    @Department NVARCHAR(50) = NULL
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT 
        r.LogDateTime,
        r.PatientIdCardNumber,
        r.PatientName,
        r.ReasonText,
        r.Department,
        r.InformedPerson,
        u.FullName AS LoggedByUsername
    FROM [dbo].[RepeatSampleLog] r
    LEFT JOIN [dbo].[Users] u ON r.LoggedByUserID = u.UserID
    WHERE r.IsActive = 1 
      AND r.LogDateTime >= @StartDate 
      AND r.LogDateTime < @InclusiveEndDate
      AND (@Reason IS NULL OR @Reason = 'All' OR r.ReasonText = @Reason)
      AND (@Department IS NULL OR @Department = 'All' OR r.Department = @Department)
    ORDER BY r.LogDateTime DESC;
END
