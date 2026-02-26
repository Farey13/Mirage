CREATE PROCEDURE [dbo].[usp_Handovers_GetReportData]
    @StartDate DATE,
    @EndDate DATE,
    @Shift NVARCHAR(50) = NULL,
    @Priority NVARCHAR(50) = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT 
        h.GivenDateTime,
        givenBy.FullName AS GivenByUsername,
        h.Shift,
        h.Priority,
        h.HandoverNotes,
        h.IsReceived,
        h.ReceivedDateTime,
        receivedBy.FullName AS ReceivedByUsername
    FROM [dbo].[Handovers] h
    LEFT JOIN [dbo].[Users] givenBy ON h.GivenByUserID = givenBy.UserID
    LEFT JOIN [dbo].[Users] receivedBy ON h.ReceivedByUserID = receivedBy.UserID
    WHERE h.IsActive = 1
      AND h.GivenDateTime >= @StartDate 
      AND h.GivenDateTime < @InclusiveEndDate
      AND (@Shift IS NULL OR @Shift = 'All' OR h.Shift = @Shift)
      AND (@Priority IS NULL OR @Priority = 'All' OR h.Priority = @Priority)
      AND (@Status IS NULL 
           OR (@Status = 'Pending' AND h.IsReceived = 0) 
           OR (@Status = 'Received' AND h.IsReceived = 1))
    ORDER BY h.GivenDateTime DESC;
END
