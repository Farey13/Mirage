CREATE PROCEDURE [dbo].[usp_RolePermissions_RemovePermissionFromRole]
    @RoleId INT,
    @PermissionId INT
AS
BEGIN
        SET XACT_ABORT ON;
    DELETE FROM [dbo].[RolePermissions] 
    WHERE RoleID = @RoleId 
      AND PermissionID = @PermissionId;
END
