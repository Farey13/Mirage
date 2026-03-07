using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PortalMirage.Data;

public class TaskRepository(IDbConnectionFactory connectionFactory) : ITaskRepository
{
    public async Task<IEnumerable<TaskModel>> GetAllAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<TaskModel>(
            "usp_Tasks_GetAll",
            new { IsDeleted = false },
            commandType: CommandType.StoredProcedure);
    }

    public async System.Threading.Tasks.Task DeactivateAsync(int taskId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(
            "usp_Tasks_Deactivate",
            new { TaskId = taskId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<TaskModel> CreateAsync(TaskModel task)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleAsync<TaskModel>(
            "usp_Tasks_Create",
            new { TaskName = task.TaskName, ShiftID = task.ShiftID, ScheduleType = task.ScheduleType, ScheduleValue = task.ScheduleValue, IsActive = task.IsActive },
            commandType: CommandType.StoredProcedure);
    }
    
    public async Task<TaskModel?> GetByIdAsync(int taskId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<TaskModel>(
            "usp_Tasks_GetById",
            new { TaskId = taskId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<TaskModel>> GetByIdsAsync(IEnumerable<int> taskIds)
    {
        if (!taskIds.Any()) return Enumerable.Empty<TaskModel>();
        using var connection = await connectionFactory.CreateConnectionAsync();
        var taskIdsCsv = string.Join(",", taskIds);
        return await connection.QueryAsync<TaskModel>(
            "usp_Tasks_GetByIds",
            new { TaskIds = taskIdsCsv },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> SoftDeleteAsync(int taskId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var affected = await connection.ExecuteAsync(
            "UPDATE Tasks SET IsDeleted = 1 WHERE TaskID = @TaskID",
            new { TaskID = taskId });
        return affected > 0;
    }

    public async Task<bool> RestoreAsync(int taskId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var affected = await connection.ExecuteAsync(
            "UPDATE Tasks SET IsDeleted = 0 WHERE TaskID = @TaskID",
            new { TaskID = taskId });
        return affected > 0;
    }

    public async Task<bool> UpdateAsync(TaskModel task)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        var affected = await connection.ExecuteAsync(
            @"UPDATE Tasks 
              SET TaskName = @TaskName, 
                  ShiftID = @ShiftID, 
                  ScheduleType = @ScheduleType, 
                  ScheduleValue = @ScheduleValue, 
                  IsActive = @IsActive 
              WHERE TaskID = @TaskID",
            task);
        return affected > 0;
    }
}
