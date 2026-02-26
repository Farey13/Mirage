CREATE TABLE [dbo].[Tasks](
    [TaskID] [int] IDENTITY(1,1) NOT NULL,
    [TaskName] [nvarchar](255) NOT NULL,
    [TaskCategory] [nvarchar](50) NULL,
    [ScheduleType] [nvarchar](50) NOT NULL,
    [ScheduleValue] [nvarchar](50) NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [ShiftID] [int] NULL,
    CONSTRAINT [PK_Tasks] PRIMARY KEY CLUSTERED ([TaskID] ASC),
    CONSTRAINT [FK_Tasks_Shifts] FOREIGN KEY ([ShiftID]) REFERENCES [dbo].[Shifts]([ShiftID])
)
