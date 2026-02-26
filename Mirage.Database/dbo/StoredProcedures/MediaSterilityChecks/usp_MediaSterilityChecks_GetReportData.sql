CREATE PROCEDURE [dbo].[usp_MediaSterilityChecks_GetReportData]
    @StartDate DATE,
    @EndDate DATE,
    @MediaName NVARCHAR(255) = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT 
        m.CheckDateTime,
        m.MediaName,
        m.MediaLotNumber,
        m.MediaQuantity,
        m.Result37C,
        m.Result25C,
        m.OverallStatus,
        m.Comments,
        u.FullName AS PerformedByUsername
    FROM [dbo].[MediaSterilityChecks] m
    LEFT JOIN [dbo].[Users] u ON m.PerformedByUserID = u.UserID
    WHERE m.IsActive = 1 
      AND m.CheckDateTime >= @StartDate 
      AND m.CheckDateTime < @InclusiveEndDate
      AND (@MediaName IS NULL OR @MediaName = 'All' OR m.MediaName = @MediaName)
      AND (@Status IS NULL OR @Status = 'All' OR m.OverallStatus = @Status)
    ORDER BY m.CheckDateTime DESC;
END
