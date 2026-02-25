CREATE PROCEDURE [dbo].[usp_MachineBreakdowns_GetPendingCount]
AS
BEGIN
    
    SELECT COUNT(*) 
    FROM [dbo].[MachineBreakdowns] 
    WHERE IsResolved = 0 
      AND IsActive = 1;
END
