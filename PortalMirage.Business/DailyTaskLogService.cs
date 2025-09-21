using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using TaskModel = PortalMirage.Core.Models.Task;

namespace PortalMirage.Business;

public class DailyTaskLogService(
    ITaskRepository taskRepository,
    IDailyTaskLogRepository dailyTaskLogRepository) : IDailyTaskLogService
{
    public async Task<IEnumerable<TaskLogDetailDto>> GetTasksForDateAsync(DateTime date) // Changed from DateOnly
    {
        var dateOnly = DateOnly.FromDateTime(date);
        var existingLogs = (await dailyTaskLogRepository.GetForDateAsync(date)).ToList();

        if (!existingLogs.Any())
        {
            var allScheduledTasks = await taskRepository.GetAllAsync();
            var tasksForToday = allScheduledTasks.Where(task => IsTaskScheduledForToday(task, dateOnly)).ToList();

            foreach (var task in tasksForToday)
            {
                var newLog = new DailyTaskLog { TaskID = task.TaskID, LogDate = date.Date, Status = "Pending" };
                var createdLog = await dailyTaskLogRepository.CreateAsync(newLog);
                existingLogs.Add(createdLog);
            }
        }

        var allTasks = (await taskRepository.GetAllAsync()).ToDictionary(t => t.TaskID);

        return existingLogs.Select(log => new TaskLogDetailDto
        {
            LogID = log.LogID,
            TaskName = allTasks[log.TaskID].TaskName,
            TaskCategory = allTasks[log.TaskID].TaskCategory,
            Status = log.Status,
            CompletedDateTime = log.CompletedDateTime,
            CompletedByUserID = log.CompletedByUserID
        });
    }

    public async Task<TaskLogDetailDto?> UpdateTaskStatusAsync(long logId, string status, int userId)
    {
        var updatedLog = await dailyTaskLogRepository.UpdateStatusAsync(logId, status, userId);
        if (updatedLog is null) return null;

        var allTasks = (await taskRepository.GetAllAsync()).ToDictionary(t => t.TaskID);

        return new TaskLogDetailDto
        {
            LogID = updatedLog.LogID,
            TaskName = allTasks[updatedLog.TaskID].TaskName,
            TaskCategory = allTasks[updatedLog.TaskID].TaskCategory,
            Status = updatedLog.Status,
            CompletedDateTime = updatedLog.CompletedDateTime,
            CompletedByUserID = updatedLog.CompletedByUserID
        };
    }

    private bool IsTaskScheduledForToday(TaskModel task, DateOnly date)
    {
        return task.ScheduleType switch
        {
            "Daily" => true,
            "Weekly" => task.ScheduleValue == date.DayOfWeek.ToString(),
            "Monthly" => task.ScheduleValue == date.Day.ToString(),
            _ => false
        };
    }
}