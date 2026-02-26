CREATE PROCEDURE [dbo].[usp_AuditLog_GetByDateRange]
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT 
        a.AuditID, 
        a.UserID, 
        u.FullName AS UserFullName, 
        a.Timestamp,
        a.ActionType, 
        a.ModuleName, 
        a.RecordID, 
        a.FieldName,
        a.OldValue, 
        a.NewValue
    FROM [dbo].[AuditLog] a
    LEFT JOIN [dbo].[Users] u ON a.UserID = u.UserID
    WHERE a.Timestamp >= @StartDate 
      AND a.Timestamp < @InclusiveEndDate
    ORDER BY a.Timestamp DESC;
END
