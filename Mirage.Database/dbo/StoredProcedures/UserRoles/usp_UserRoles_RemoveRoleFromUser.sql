CREATE PROCEDURE [dbo].[usp_UserRoles_RemoveRoleFromUser]
    @UserId INT,
    @RoleId INT
AS
BEGIN
        SET XACT_ABORT ON;
    DELETE FROM [dbo].[UserRoles] 
    WHERE UserID = @UserId 
      AND RoleID = @RoleId;
END
