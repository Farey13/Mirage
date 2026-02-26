CREATE TABLE [dbo].[Permissions](
    [PermissionID] [int] IDENTITY(1,1) NOT NULL,
    [PermissionName] [nvarchar](255) NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED ([PermissionID] ASC),
    CONSTRAINT [UQ_Permissions_PermissionName] UNIQUE NONCLUSTERED ([PermissionName] ASC)
)
