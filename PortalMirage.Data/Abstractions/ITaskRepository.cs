using PortalMirage.Core.Models;
using TaskModel = PortalMirage.Core.Models.Task; // Creates a nickname for our Task model

namespace PortalMirage.Data.Abstractions;

public interface ITaskRepository
{
    Task<IEnumerable<TaskModel>> GetAllAsync();
    Task<TaskModel> CreateAsync(TaskModel task);
}