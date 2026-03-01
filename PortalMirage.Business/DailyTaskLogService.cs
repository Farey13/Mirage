using PortalMirage.Business.Abstractions;
using PortalMirage.Data.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskModel = PortalMirage.Core.Models.TaskModel;
using PortalMirage.Core.Models;

namespace PortalMirage.Business;

public class DailyTaskLogService : IDailyTaskLogService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IDailyTaskLogRepository _dailyTaskLogRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger<DailyTaskLogService> _logger;

    public DailyTaskLogService(
        ITaskRepository taskRepository,
        IDailyTaskLogRepository dailyTaskLogRepository,
        IShiftRepository shiftRepository,
        IUserRepository userRepository,
        IAuditLogService auditLogService,
        ITimeProvider timeProvider,
        ILogger<DailyTaskLogService> logger)
    {
        _taskRepository = taskRepository;
        _dailyTaskLogRepository = dailyTaskLogRepository;
        _shiftRepository = shiftRepository;
        _userRepository = userRepository;
        _auditLogService = auditLogService;
        _timeProvider = timeProvider;
        _logger = logger;
    }

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
        _logger.LogInformation("Fetching tasks for date: {Date}", date);
        
        if (date.Date > _timeProvider.Today)
            throw new ArgumentException("Cannot get tasks for future dates", nameof(date));

        await LockExpiredTasks(date);

        var existingLogs = (await _dailyTaskLogRepository.GetForDateAsync(date)).ToList();

        if (!existingLogs.Any())
        {
            _logger.LogInformation("No task logs found for {Date}, creating daily logs", date);
            existingLogs = await CreateDailyLogsForDate(date);
        }

        return await MapToTaskLogDetailDtos(existingLogs);
    }

    public async Task<TaskLogDetailDto?> UpdateTaskStatusAsync(long logId, string status, int userId, string? comment = null)
    {
        _logger.LogInformation("Updating task log {LogId} status to {Status} by user {UserId}", logId, status, userId);
        
        if (!IsValidStatus(status))
            throw new ArgumentException($"Invalid status: {status}", nameof(status));

        if (userId <= 0)
            throw new ArgumentException("Invalid user ID", nameof(userId));

        var updatedLog = await _dailyTaskLogRepository.UpdateStatusAsync(logId, status, userId, comment);
        if (updatedLog is null)
        {
            _logger.LogWarning("Task log not found: {LogId}", logId);
            return null;
        }

        await _auditLogService.LogAsync(
            userId: userId,
            actionType: AuditActions.UpdateStatus,
            moduleName: "DailyTaskLog",
            recordId: logId.ToString(),
            newValue: $"Status set to '{status}'. Comment: {comment ?? "N/A"}"
        );

        _logger.LogInformation("Task log {LogId} status updated to {Status}", logId, status);
        return await MapToTaskLogDetailDto(updatedLog, userId);
    }

    public async Task<DailyTaskLog?> ExtendTaskDeadlineAsync(long logId, DateTime newDeadline, string reason, int adminUserId)
    {
        _logger.LogInformation("Extending deadline for task log {LogId} to {NewDeadline} by admin {AdminUserId}", 
            logId, newDeadline, adminUserId);
        
        if (newDeadline <= _timeProvider.Now)
            throw new ArgumentException("New deadline must be in the future", nameof(newDeadline));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required", nameof(reason));

        var extendedLog = await _dailyTaskLogRepository.ExtendDeadlineAsync(logId, newDeadline, reason, adminUserId);
        if (extendedLog is not null)
        {
            await _auditLogService.LogAsync(
                adminUserId,
                AuditActions.ExtendDeadline,
                nameof(DailyTaskLog),
                logId.ToString(),
                newValue: $"Deadline extended to {newDeadline:yyyy-MM-dd HH:mm} for reason: {reason}");
            
            _logger.LogInformation("Task log {LogId} deadline extended to {NewDeadline}", logId, newDeadline);
        }
        return extendedLog;
    }

    public async Task<DailyTaskLog?> MarkAsNotApplicableAsync(long logId, int userId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required", nameof(reason));

        _logger.LogInformation("Marking task log {LogId} as not applicable by user {UserId}", logId, userId);
        
        var updatedLog = await _dailyTaskLogRepository.UpdateStatusAsync(logId, TaskStatuses.NotApplicable, userId, reason);
        if (updatedLog is not null)
        {
            await _auditLogService.LogAsync(
                userId,
                AuditActions.MarkAsNA,
                nameof(DailyTaskLog),
                logId.ToString(),
                newValue: $"Marked as Not Applicable: {reason}");
            
            _logger.LogInformation("Task log {LogId} marked as not applicable", logId);
        }
        return updatedLog;
    }

    public async Task<DailyTaskLog?> OverrideLockAsync(long logId, DateTime overrideUntil, string reason, int adminUserId)
    {
        if (overrideUntil <= _timeProvider.Now)
            throw new ArgumentException("Override must be in the future", nameof(overrideUntil));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required", nameof(reason));

        _logger.LogInformation("Overriding lock for task log {LogId} until {OverrideUntil} by admin {AdminUserId}", 
            logId, overrideUntil, adminUserId);
        
        var updatedLog = await _dailyTaskLogRepository.OverrideLockAsync(logId, overrideUntil, reason, adminUserId);
        if (updatedLog is not null)
        {
            await _auditLogService.LogAsync(
                adminUserId,
                AuditActions.OverrideLock,
                nameof(DailyTaskLog),
                logId.ToString(),
                newValue: $"Lock override until {overrideUntil:yyyy-MM-dd HH:mm} for reason: {reason}");
            
            _logger.LogInformation("Task log {LogId} lock overridden until {OverrideUntil}", logId, overrideUntil);
        }
        return updatedLog;
    }

    #region Private Methods
    private async Task<List<DailyTaskLog>> CreateDailyLogsForDate(DateTime date)
    {
        var logs = new List<DailyTaskLog>();
        var allScheduledTasks = await _taskRepository.GetAllAsync();
        var tasksForToday = allScheduledTasks.Where(task => IsTaskScheduledForToday(task, date)).ToList();

        _logger.LogInformation("Creating daily logs for {Count} tasks scheduled for {Date}", tasksForToday.Count, date);

        foreach (var task in tasksForToday)
        {
            var newLog = new DailyTaskLog
            {
                TaskID = task.TaskID,
                LogDate = date.Date,
                Status = TaskStatuses.Pending
            };
            var createdLog = await _dailyTaskLogRepository.CreateAsync(newLog);
            logs.Add(createdLog);
        }

        return logs;
    }

    private async System.Threading.Tasks.Task LockExpiredTasks(DateTime forDate)
    {
        if (forDate.Date > _timeProvider.Today) return;

        var allShifts = await _shiftRepository.GetAllAsync();
        var pendingLogs = (await _dailyTaskLogRepository.GetForDateAsync(forDate))
            .Where(log => log.Status == TaskStatuses.Pending).ToList();

        if (!pendingLogs.Any()) return;

        var allTasks = (await _taskRepository.GetAllAsync()).ToDictionary(t => t.TaskID);

        foreach (var log in pendingLogs)
        {
            if (!allTasks.TryGetValue(log.TaskID, out var task) || !task.ShiftID.HasValue)
                continue;

            var shift = allShifts.FirstOrDefault(s => s.ShiftID == task.ShiftID.Value);
            if (shift is null) continue;

            var shiftEndTimeOnDate = forDate.Date + shift.EndTime.ToTimeSpan();
            var lockTime = shiftEndTimeOnDate.AddHours(shift.GracePeriodHours);

            if (log.LockOverrideUntil.HasValue && log.LockOverrideUntil.Value > _timeProvider.Now)
                continue;

            if (_timeProvider.Now > lockTime)
            {
                await _dailyTaskLogRepository.UpdateStatusAsync(
                    log.LogID,
                    TaskStatuses.Incomplete,
                    null,
                    "Automatically locked due to expired deadline");

                await _auditLogService.LogAsync(
                    0,
                    AuditActions.AutoLock,
                    nameof(DailyTaskLog),
                    log.LogID.ToString(),
                    newValue: $"Automatically locked at {_timeProvider.Now:yyyy-MM-dd HH:mm} - deadline was {lockTime:yyyy-MM-dd HH:mm}");
                
                _logger.LogInformation("Task log {LogID} automatically locked due to expired deadline", log.LogID);
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

        var tasks = (await _taskRepository.GetByIdsAsync(taskIds)).ToDictionary(t => t.TaskID);
        var users = (await _userRepository.GetByIdsAsync(userIds)).ToDictionary(u => u.UserID);

        var shifts = (await _shiftRepository.GetAllAsync()).ToDictionary(s => s.ShiftID);

        return logList.Select(log => MapToTaskLogDetailDto(log, tasks, users, shifts));
    }

    private async Task<TaskLogDetailDto> MapToTaskLogDetailDto(DailyTaskLog log, int? completedByUserId = null)
    {
        var task = await _taskRepository.GetByIdAsync(log.TaskID);
        var user = completedByUserId.HasValue ? await _userRepository.GetByIdAsync(completedByUserId.Value) : null;

        var shifts = (await _shiftRepository.GetAllAsync()).ToDictionary(s => s.ShiftID);
        string categoryName = "Uncategorized";

        if (task != null && task.ShiftID.HasValue && shifts.TryGetValue(task.ShiftID.Value, out var shift))
        {
            categoryName = shift.ShiftName;
        }

        return new TaskLogDetailDto
        {
            LogID = log.LogID,
            TaskName = task?.TaskName ?? "Unknown Task",
            TaskCategory = categoryName,
            Status = log.Status,
            CompletedDateTime = log.CompletedDateTime,
            CompletedByUserID = log.CompletedByUserID,
            CompletedByUsername = user?.FullName ?? "Unknown User",
            Comments = log.Comments,
            LockOverrideUntil = log.LockOverrideUntil
        };
    }

    private TaskLogDetailDto MapToTaskLogDetailDto(
        DailyTaskLog log,
        Dictionary<int, TaskModel> tasks,
        Dictionary<int, User> users,
        Dictionary<int, Shift> shifts)
    {
        var task = tasks.TryGetValue(log.TaskID, out var t) ? t : null;
        var user = log.CompletedByUserID.HasValue && users.TryGetValue(log.CompletedByUserID.Value, out var u) ? u : null;

        string categoryName = "Uncategorized";
        if (task != null && task.ShiftID.HasValue && shifts.TryGetValue(task.ShiftID.Value, out var shift))
        {
            categoryName = shift.ShiftName;
        }

        return new TaskLogDetailDto
        {
            LogID = log.LogID,
            TaskName = task?.TaskName ?? "Unknown Task",
            TaskCategory = categoryName,
            Status = log.Status,
            CompletedDateTime = log.CompletedDateTime,
            CompletedByUserID = log.CompletedByUserID,
            CompletedByUsername = user?.FullName ?? "Unknown User",
            Comments = log.Comments,
            LockOverrideUntil = log.LockOverrideUntil
        };
    }

    private TaskLogDetailDto MapToTaskLogDetailDto(DailyTaskLog log, Dictionary<int, TaskModel> tasks, Dictionary<int, User> users)
    {
        return MapToTaskLogDetailDto(log, tasks, users, new Dictionary<int, Shift>());
    }

    private bool IsValidStatus(string status)
    {
        var validStatuses = new[] { TaskStatuses.Pending, TaskStatuses.Complete, TaskStatuses.Incomplete, TaskStatuses.NotApplicable };
        return validStatuses.Contains(status);
    }
    #endregion
}