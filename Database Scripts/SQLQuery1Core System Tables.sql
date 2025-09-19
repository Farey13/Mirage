-- =================================================================
-- Phase 1, Step 1.1 (Part 1): Core System Tables
-- Users, Roles, Permissions, and AuditLog for MirageDB
-- =================================================================

-- 1. Users Table: Stores user login information
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(255) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 2. Roles Table: Defines the user roles (e.g., Admin, Technician)
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(100) NOT NULL UNIQUE
);
GO

-- 3. UserRoles Table: Links Users to Roles (Many-to-Many)
CREATE TABLE UserRoles (
    UserID INT NOT NULL,
    RoleID INT NOT NULL,
    PRIMARY KEY (UserID, RoleID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID) ON DELETE CASCADE
);
GO

-- 4. Permissions Table: Defines individual permissions
CREATE TABLE Permissions (
    PermissionID INT IDENTITY(1,1) PRIMARY KEY,
    PermissionName NVARCHAR(255) NOT NULL UNIQUE -- e.g., 'KitValidation.Create', 'Users.Edit'
);
GO

-- 5. RolePermissions Table: Links Roles to Permissions (Many-to-Many)
CREATE TABLE RolePermissions (
    RoleID INT NOT NULL,
    PermissionID INT NOT NULL,
    PRIMARY KEY (RoleID, PermissionID),
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID) ON DELETE CASCADE,
    FOREIGN KEY (PermissionID) REFERENCES Permissions(PermissionID) ON DELETE CASCADE
);
GO

-- 6. AuditLog Table: Records all changes to data
CREATE TABLE AuditLog (
    AuditID BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserID INT,
    Timestamp DATETIME2 NOT NULL DEFAULT GETDATE(),
    ActionType NVARCHAR(50) NOT NULL,  -- e.g., 'Create', 'Update', 'Delete', 'Login'
    ModuleName NVARCHAR(100),
    RecordID NVARCHAR(100),
    FieldName NVARCHAR(100),
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE SET NULL
);
GO