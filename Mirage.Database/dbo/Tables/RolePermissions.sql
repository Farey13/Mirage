CREATE TABLE [dbo].[RolePermissions](
    [RoleID] [int] NOT NULL,
    [PermissionID] [int] NOT NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED ([RoleID] ASC, [PermissionID] ASC),
    CONSTRAINT [FK_RolePermissions_Roles] FOREIGN KEY ([RoleID]) REFERENCES [dbo].[Roles]([RoleID]) ON DELETE CASCADE,
    CONSTRAINT [FK_RolePermissions_Permissions] FOREIGN KEY ([PermissionID]) REFERENCES [dbo].[Permissions]([PermissionID]) ON DELETE CASCADE
)
