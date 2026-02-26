CREATE PROCEDURE [dbo].[usp_Users_GetUserRoles]
    @UserId INT
AS
BEGIN
    
    SELECT r.RoleName 
    FROM [dbo].[Roles] r
    INNER JOIN [dbo].[UserRoles] ur ON r.RoleID = ur.RoleID
    WHERE ur.UserID = @UserId;
END
