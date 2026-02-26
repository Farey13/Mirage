CREATE TABLE [dbo].[AdminListItems](
    [ItemID] [int] IDENTITY(1,1) NOT NULL,
    [ListType] [nvarchar](100) NOT NULL,
    [ItemValue] [nvarchar](255) NOT NULL,
    [Description] [nvarchar](500) NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    CONSTRAINT [PK_AdminListItems] PRIMARY KEY CLUSTERED ([ItemID] ASC)
)
