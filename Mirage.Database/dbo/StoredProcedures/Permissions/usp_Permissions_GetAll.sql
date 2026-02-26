CREATE PROCEDURE [dbo].[usp_Permissions_GetAll]
AS
BEGIN
    
    SELECT PermissionID, PermissionName
    FROM [dbo].[Permissions];
END
