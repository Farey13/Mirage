using PortalMirage.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace PortalMirage.Data.Abstractions;

public interface ITaskRepository
{
    Task<IEnumerable<TaskModel>> GetAllAsync();
    Task<TaskModel> CreateAsync(TaskModel task);
    Task<IEnumerable<TaskModel>> GetByIdsAsync(IEnumerable<int> taskIds);
    Task<TaskModel?> GetByIdAsync(int taskId);
    Task DeactivateAsync(int taskId);
    Task<bool> SoftDeleteAsync(int taskId);
    Task<bool> RestoreAsync(int taskId);
    Task<bool> UpdateAsync(TaskModel task);
}