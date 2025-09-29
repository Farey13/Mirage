using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class ShiftService(IShiftRepository shiftRepository) : IShiftService
{
    public async Task<IEnumerable<Shift>> GetAllAsync()
    {
        return await shiftRepository.GetAllAsync();
    }

    public async Task<Shift?> GetByIdAsync(int shiftId)
    {
        return await shiftRepository.GetByIdAsync(shiftId);
    }

    public async Task<Shift> CreateAsync(Shift shift)
    {
        // In the future, we could add business logic here,
        // like preventing overlapping shift times.
        return await shiftRepository.CreateAsync(shift);
    }

    public async Task<Shift> UpdateAsync(Shift shift)
    {
        return await shiftRepository.UpdateAsync(shift);
    }

    public async System.Threading.Tasks. Task DeactivateAsync(int shiftId)
    {
        await shiftRepository.DeactivateAsync(shiftId);
    }
}