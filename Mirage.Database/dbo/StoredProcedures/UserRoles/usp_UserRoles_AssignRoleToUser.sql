CREATE PROCEDURE [dbo].[usp_UserRoles_AssignRoleToUser]
    @UserId INT,
    @RoleId INT
AS
BEGIN
        SET XACT_ABORT ON;
    INSERT INTO [dbo].[UserRoles] (UserID, RoleID)
    VALUES (@UserId, @RoleId);
END
