CREATE PROCEDURE [dbo].[usp_Handovers_GetCompleted]
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    
    DECLARE @InclusiveEndDate DATE = DATEADD(day, 1, @EndDate);
    SELECT HandoverID, HandoverNotes, GivenDateTime, GivenByUserID, IsReceived, 
           ReceivedDateTime, ReceivedByUserID, Priority, Shift, IsActive, 
           DeactivationReason, DeactivatedByUserID, DeactivationDateTime
    FROM [dbo].[Handovers] 
    WHERE IsReceived = 1 
      AND IsActive = 1 
      AND GivenDateTime >= @StartDate 
      AND GivenDateTime < @InclusiveEndDate
    ORDER BY GivenDateTime DESC;
END
