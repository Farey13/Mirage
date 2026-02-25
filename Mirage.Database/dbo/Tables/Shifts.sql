CREATE TABLE [dbo].[Shifts](
    [ShiftID] [int] IDENTITY(1,1) NOT NULL,
    [ShiftName] [nvarchar](100) NOT NULL,
    [StartTime] [time](7) NOT NULL,
    [EndTime] [time](7) NOT NULL,
    [GracePeriodHours] [int] NOT NULL DEFAULT 2,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Shifts] PRIMARY KEY CLUSTERED ([ShiftID] ASC),
    CONSTRAINT [UQ_Shifts_ShiftName] UNIQUE NONCLUSTERED ([ShiftName] ASC)
)
