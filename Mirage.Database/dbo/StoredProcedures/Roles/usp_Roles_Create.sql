CREATE PROCEDURE [dbo].[usp_Roles_Create]
    @RoleName NVARCHAR(100)
AS
BEGIN
        SET XACT_ABORT ON;
    INSERT INTO [dbo].[Roles] (RoleName)
    OUTPUT INSERTED.RoleID, INSERTED.RoleName
    VALUES (@RoleName);
END
