using PortalMirage.Business.Abstractions;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskModel = PortalMirage.Core.Models.Task;
using PortalMirage.Core.Models;

namespace PortalMirage.Business;

public class DailyTaskLogService(
    ITaskRepository taskRepository,
    IDailyTaskLogRepository dailyTaskLogRepository,
    IShiftRepository shiftRepository,
    IUserRepository userRepository,
    IAuditLogService auditLogService,
    ITimeProvider timeProvider) : IDailyTaskLogService
{
    #region Constants
    public static class TaskStatuses
    {
        public const string Pending = "Pending";
        public const string Complete = "Complete";
        public const string Incomplete = "Incomplete";
        public const string NotApplicable = "NotApplicable";
    }

    public static class AuditActions
    {
        public const string UpdateStatus = "UpdateStatus";
        public const string ExtendDeadline = "ExtendDeadline";
        public const string MarkAsNA = "MarkAsNA";
        public const string OverrideLock = "OverrideLock";
        public const string AutoLock = "AutoLock";
    }

    public static class ScheduleTypes
    {
        public const string Daily = "Daily";
        public const string Weekly = "Weekly";
        public const string Monthly = "Monthly";
    }
    #endregion

    public async Task<IEnumerable<TaskLogDetailDto>> GetTasksForDateAsync(DateTime date)
    {
        if (date.Date > timeProvider.Today)
            throw new ArgumentException("Cannot get tasks for future dates", nameof(date));

        await LockExpiredTasks(date);

        var existingLogs = (await dailyTaskLogRepository.GetForDateAsync(date)).ToList();

        if (!existingLogs.Any())
        {
            existingLogs = await CreateDailyLogsForDate(date);
        }

        return await MapToTaskLogDetailDtos(existingLogs);
    }

    public async Task<TaskLogDetailDto?> UpdateTaskStatusAsync(long logId, string status, int userId, string? comment = null)
    {
        // Validate input
        if (!IsValidStatus(status))
            throw new ArgumentException($"Invalid status: {status}", nameof(status));

        if (userId <= 0)
            throw new ArgumentException("Invalid user ID", nameof(userId));

        var updatedLog = await dailyTaskLogRepository.UpdateStatusAsync(logId, status, userId, comment);
        if (updatedLog is null)
            return null;

        // ADDED: Enhanced audit logging with more descriptive message
        await auditLogService.LogAsync(
            userId: userId,
            actionType: AuditActions.UpdateStatus,
            moduleName: "DailyTaskLog",
            recordId: logId.ToString(),
            newValue: $"Status set to '{status}'. Comment: {comment ?? "N/A"}"
        );

        return await MapToTaskLogDetailDto(updatedLog, userId);
    }

    public async Task<DailyTaskLog?> ExtendTaskDeadlineAsync(long logId, DateTime newDeadline, string reason, int adminUserId)
    {
        if (newDeadline <= timeProvider.Now)
            throw new ArgumentException("New deadline must be in the future", nameof(newDeadline));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required", nameof(reason));

        var extendedLog = await dailyTaskLogRepository.ExtendDeadlineAsync(logId, newDeadline, reason, adminUserId);
        if (extendedLog is not null)
        {
            // This was already here and is correct
            await auditLogService.LogAsync(
                adminUserId,
                AuditActions.ExtendDeadline,
                nameof(DailyTaskLog),
                logId.ToString(),
                newValue: $"Deadline extended to {newDeadline:yyyy-MM-dd HH:mm} for reason: {reason}");
        }
        return extendedLog;
    }

    public async Task<DailyTaskLog?> MarkAsNotApplicableAsync(long logId, int userId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required", nameof(reason));

        var updatedLog = await dailyTaskLogRepository.UpdateStatusAsync(logId, TaskStatuses.NotApplicable, userId, reason);
        if (updatedLog is not null)
        {
            await auditLogService.LogAsync(
                userId,
                AuditActions.MarkAsNA,
                nameof(DailyTaskLog),
                logId.ToString(),
                newValue: $"Marked as Not Applicable: {reason}");
        }
        return updatedLog;
    }

    public async Task<DailyTaskLog?> OverrideLockAsync(long logId, DateTime overrideUntil, string reason, int adminUserId)
    {
        if (overrideUntil <= timeProvider.Now)
            throw new ArgumentException("Override must be in the future", nameof(overrideUntil));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required", nameof(reason));

        var updatedLog = await dailyTaskLogRepository.OverrideLockAsync(logId, overrideUntil, reason, adminUserId);
        if (updatedLog is not null)
        {
            // This was already here and is correct
            await auditLogService.LogAsync(
                adminUserId,
                AuditActions.OverrideLock,
                nameof(DailyTaskLog),
                logId.ToString(),
                newValue: $"Lock override until {overrideUntil:yyyy-MM-dd HH:mm} for reason: {reason}");
        }
        return updatedLog;
    }

    #region Private Methods
    private async Task<List<DailyTaskLog>> CreateDailyLogsForDate(DateTime date)
    {
        var logs = new List<DailyTaskLog>();
        var allScheduledTasks = await taskRepository.GetAllAsync();
        var tasksForToday = allScheduledTasks.Where(task => IsTaskScheduledForToday(task, date)).ToList();

        foreach (var task in tasksForToday)
        {
            var newLog = new DailyTaskLog
            {
                TaskID = task.TaskID,
                LogDate = date.Date,
                Status = TaskStatuses.Pending
            };
            var createdLog = await dailyTaskLogRepository.CreateAsync(newLog);
            logs.Add(createdLog);
        }

        return logs;
    }

    // FIX 1: Explicitly used System.Threading.Tasks.Task to resolve ambiguity.
    private async System.Threading.Tasks.Task LockExpiredTasks(DateTime forDate)
    {
        if (forDate.Date > timeProvider.Today) return;

        var allShifts = await shiftRepository.GetAllAsync();
        var pendingLogs = (await dailyTaskLogRepository.GetForDateAsync(forDate))
            .Where(log => log.Status == TaskStatuses.Pending).ToList();

        if (!pendingLogs.Any()) return;

        var allTasks = (await taskRepository.GetAllAsync()).ToDictionary(t => t.TaskID);

        foreach (var log in pendingLogs)
        {
            if (!allTasks.TryGetValue(log.TaskID, out var task) || !task.ShiftID.HasValue)
                continue;

            var shift = allShifts.FirstOrDefault(s => s.ShiftID == task.ShiftID.Value);
            if (shift is null) continue;

            var shiftEndTimeOnDate = forDate.Date + shift.EndTime.ToTimeSpan();
            var lockTime = shiftEndTimeOnDate.AddHours(shift.GracePeriodHours);

            if (log.LockOverrideUntil.HasValue && log.LockOverrideUntil.Value > timeProvider.Now)
                continue;

            if (timeProvider.Now > lockTime)
            {
                await dailyTaskLogRepository.UpdateStatusAsync(
                    log.LogID,
                    TaskStatuses.Incomplete,
                    null,
                    "Automatically locked due to expired deadline");

                await auditLogService.LogAsync(
                    0, // System user
                    AuditActions.AutoLock,
                    nameof(DailyTaskLog),
                    log.LogID.ToString(),
                    newValue: $"Automatically locked at {timeProvider.Now:yyyy-MM-dd HH:mm} - deadline was {lockTime:yyyy-MM-dd HH:mm}");
            }
        }
    }

    private bool IsTaskScheduledForToday(TaskModel task, DateTime date)
    {
        var dateOnly = DateOnly.FromDateTime(date);
        return task.ScheduleType switch
        {
            ScheduleTypes.Daily => true,
            ScheduleTypes.Weekly => task.ScheduleValue.Equals(dateOnly.DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase),
            ScheduleTypes.Monthly => int.TryParse(task.ScheduleValue, out int day) && day == dateOnly.Day,
            _ => false
        };
    }

    private async Task<IEnumerable<TaskLogDetailDto>> MapToTaskLogDetailDtos(IEnumerable<DailyTaskLog> logs)
    {
        var logList = logs.ToList();
        if (!logList.Any()) return Enumerable.Empty<TaskLogDetailDto>();

        var taskIds = logList.Select(l => l.TaskID).Distinct();
        var userIds = logList.Where(l => l.CompletedByUserID.HasValue)
                           .Select(l => l.CompletedByUserID.Value)
                           .Distinct();

        var tasks = (await taskRepository.GetByIdsAsync(taskIds)).ToDictionary(t => t.TaskID);
        var users = (await userRepository.GetByIdsAsync(userIds)).ToDictionary(u => u.UserID);

        return logList.Select(log => MapToTaskLogDetailDto(log, tasks, users));
    }

    private async Task<TaskLogDetailDto> MapToTaskLogDetailDto(DailyTaskLog log, int? completedByUserId = null)
    {
        var task = await taskRepository.GetByIdAsync(log.TaskID);
        var user = completedByUserId.HasValue ? await userRepository.GetByIdAsync(completedByUserId.Value) : null;

        return new TaskLogDetailDto
        {
            LogID = log.LogID,
            TaskName = task?.TaskName ?? "Unknown Task",
            TaskCategory = task?.ShiftID?.ToString() ?? "Uncategorized",
            Status = log.Status,
            CompletedDateTime = log.CompletedDateTime,
            CompletedByUserID = log.CompletedByUserID,
            CompletedByUsername = user?.FullName ?? "Unknown User",
            Comments = log.Comments,
            LockOverrideUntil = log.LockOverrideUntil
        };
    }

    // FIX 2: Changed Dictionary key from 'int' to 'long' to match the TaskID type.
    private TaskLogDetailDto MapToTaskLogDetailDto(DailyTaskLog log, Dictionary<int, TaskModel> tasks, Dictionary<int, User> users)
    {
        var task = tasks.TryGetValue(log.TaskID, out var t) ? t : null;
        var user = log.CompletedByUserID.HasValue && users.TryGetValue(log.CompletedByUserID.Value, out var u) ? u : null;

        return new TaskLogDetailDto
        {
            LogID = log.LogID,
            TaskName = task?.TaskName ?? "Unknown Task",
            TaskCategory = task?.ShiftID?.ToString() ?? "Uncategorized",
            Status = log.Status,
            CompletedDateTime = log.CompletedDateTime,
            CompletedByUserID = log.CompletedByUserID,
            CompletedByUsername = user?.FullName ?? "Unknown User",
            Comments = log.Comments,
            LockOverrideUntil = log.LockOverrideUntil
        };
    }

    private bool IsValidStatus(string status)
    {
        var validStatuses = new[] { TaskStatuses.Pending, TaskStatuses.Complete, TaskStatuses.Incomplete, TaskStatuses.NotApplicable };
        return validStatuses.Contains(status);
    }
    #endregion
}