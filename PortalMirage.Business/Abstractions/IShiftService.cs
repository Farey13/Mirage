using PortalMirage.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions;

public interface IShiftService
{
    Task<IEnumerable<Shift>> GetAllAsync();
    Task<Shift?> GetByIdAsync(int shiftId);
    Task<Shift> CreateAsync(Shift shift, int actorUserId); // Add actorUserId
    Task<Shift> UpdateAsync(Shift shift, int actorUserId); // Add actorUserId
    System.Threading.Tasks.Task DeactivateAsync(int shiftId, int actorUserId); // Add actorUserId
}