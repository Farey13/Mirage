USE MirageDB;
GO

IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO Roles (RoleName) VALUES ('Admin');
END

IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserID = 1 AND RoleID = (SELECT RoleID FROM Roles WHERE RoleName = 'Admin'))
BEGIN
    INSERT INTO UserRoles (UserID, RoleID) VALUES (1, (SELECT RoleID FROM Roles WHERE RoleName = 'Admin'));
END
GO