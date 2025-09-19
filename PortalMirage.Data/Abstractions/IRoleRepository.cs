using PortalMirage.Core.Models;

namespace PortalMirage.Data.Abstractions;

public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role> CreateAsync(Role role);
}