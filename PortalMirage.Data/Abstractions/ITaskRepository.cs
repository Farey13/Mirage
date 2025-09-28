using PortalMirage.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskModel = PortalMirage.Core.Models.Task;

namespace PortalMirage.Data.Abstractions;

public interface ITaskRepository
{
    Task<IEnumerable<TaskModel>> GetAllAsync();
    Task<TaskModel> CreateAsync(TaskModel task);
    Task<IEnumerable<TaskModel>> GetByIdsAsync(IEnumerable<int> taskIds);
    Task<TaskModel?> GetByIdAsync(int taskId);
}