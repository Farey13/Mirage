using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskModel = PortalMirage.Core.Models.Task; // Alias for our model

namespace PortalMirage.Data;

public class TaskRepository(IDbConnectionFactory connectionFactory) : ITaskRepository
{
    public async Task<IEnumerable<TaskModel>> GetAllAsync()
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM Tasks WHERE IsActive = 1";
        return await connection.QueryAsync<TaskModel>(sql);
    }

    public async Task<TaskModel> CreateAsync(TaskModel task)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = """
                           INSERT INTO Tasks (TaskName, ShiftID, ScheduleType, ScheduleValue, IsActive)
                           OUTPUT INSERTED.*
                           VALUES (@TaskName, @ShiftID, @ScheduleType, @ScheduleValue, @IsActive);
                           """;
        return await connection.QuerySingleAsync<TaskModel>(sql, task);
    }
    public async Task<TaskModel?> GetByIdAsync(int taskId)
    {
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM Tasks WHERE TaskID = @TaskId";
        return await connection.QuerySingleOrDefaultAsync<TaskModel>(sql, new { TaskId = taskId });
    }

    public async Task<IEnumerable<TaskModel>> GetByIdsAsync(IEnumerable<int> taskIds)
    {
        if (!taskIds.Any()) return Enumerable.Empty<TaskModel>();
        using var connection = await connectionFactory.CreateConnectionAsync();
        const string sql = "SELECT * FROM Tasks WHERE TaskID IN @TaskIds";
        return await connection.QueryAsync<TaskModel>(sql, new { TaskIds = taskIds });
    }
}