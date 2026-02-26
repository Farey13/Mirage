CREATE PROCEDURE [dbo].[usp_SampleStorage_GetPendingCount]
AS
BEGIN
    
    SELECT COUNT(*) 
    FROM [dbo].[SampleStorage] 
    WHERE IsTestDone = 0 
      AND IsActive = 1;
END
