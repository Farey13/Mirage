CREATE PROCEDURE [dbo].[usp_UserRoles_GetRolesForUser]
    @Username NVARCHAR(100)
AS
BEGIN
    
    SELECT r.RoleID, r.RoleName
    FROM [dbo].[Roles] r
    INNER JOIN [dbo].[UserRoles] ur ON r.RoleID = ur.RoleID
    INNER JOIN [dbo].[Users] u ON ur.UserID = u.UserID
    WHERE u.Username = @Username;
END
