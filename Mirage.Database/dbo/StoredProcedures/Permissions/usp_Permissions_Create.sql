CREATE PROCEDURE [dbo].[usp_Permissions_Create]
    @PermissionName NVARCHAR(255)
AS
BEGIN
        SET XACT_ABORT ON;
    INSERT INTO [dbo].[Permissions] (PermissionName)
    OUTPUT INSERTED.PermissionID, INSERTED.PermissionName
    VALUES (@PermissionName);
END
