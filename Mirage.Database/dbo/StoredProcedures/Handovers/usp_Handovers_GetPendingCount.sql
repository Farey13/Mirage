CREATE PROCEDURE [dbo].[usp_Handovers_GetPendingCount]
AS
BEGIN
    
    SELECT COUNT(*) 
    FROM [dbo].[Handovers] 
    WHERE IsReceived = 0 
      AND IsActive = 1 
      AND GivenDateTime >= CAST(GETUTCDATE() AS DATE) 
      AND GivenDateTime < DATEADD(day, 1, CAST(GETUTCDATE() AS DATE));
END
