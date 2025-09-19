using PortalMirage.Core.Models;

namespace PortalMirage.Data.Abstractions;

public interface IPermissionRepository
{
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<Permission> CreateAsync(Permission permission);
}