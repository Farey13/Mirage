using Dapper;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using TaskModel = PortalMirage.Core.Models.Task; // Creates a nickname for our Task model

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
                           INSERT INTO Tasks (TaskName, TaskCategory, ScheduleType, ScheduleValue, IsActive)
                           OUTPUT INSERTED.*
                           VALUES (@TaskName, @TaskCategory, @ScheduleType, @ScheduleValue, @IsActive);
                           """;
        return await connection.QuerySingleAsync<TaskModel>(sql, task);
    }
}