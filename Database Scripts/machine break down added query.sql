USE MirageDB;
GO

ALTER TABLE MachineBreakdowns
ADD ResolutionNotes NVARCHAR(MAX) NULL;

ALTER TABLE MachineBreakdowns
ADD DowntimeMinutes INT NULL;
GO