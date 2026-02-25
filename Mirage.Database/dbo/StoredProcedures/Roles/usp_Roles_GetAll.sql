CREATE PROCEDURE [dbo].[usp_Roles_GetAll]
AS
BEGIN
    
    SELECT RoleID, RoleName
    FROM [dbo].[Roles];
END
