CREATE PROCEDURE [dbo].[usp_RolePermissions_AssignPermissionToRole]
    @RoleId INT,
    @PermissionId INT
AS
BEGIN
        SET XACT_ABORT ON;
    INSERT INTO [dbo].[RolePermissions] (RoleID, PermissionID)
    VALUES (@RoleId, @PermissionId);
END
