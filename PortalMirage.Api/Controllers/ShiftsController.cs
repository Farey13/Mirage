using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using System.Threading.Tasks;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // This entire controller is for Admins only
    public class ShiftsController(IShiftService shiftService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var shifts = await shiftService.GetAllAsync();
            return Ok(shifts);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateShiftRequest request)
        {
            var newShift = new Shift
            {
                ShiftName = request.ShiftName,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                GracePeriodHours = request.GracePeriodHours,
                IsActive = true
            };
            var createdShift = await shiftService.CreateAsync(newShift);
            return Ok(createdShift);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateShiftRequest request)
        {
            if (id != request.ShiftID) return BadRequest("ID mismatch");

            var shiftToUpdate = await shiftService.GetByIdAsync(id);
            if (shiftToUpdate is null) return NotFound();

            shiftToUpdate.ShiftName = request.ShiftName;
            shiftToUpdate.StartTime = request.StartTime;
            shiftToUpdate.EndTime = request.EndTime;
            shiftToUpdate.GracePeriodHours = request.GracePeriodHours;
            shiftToUpdate.IsActive = request.IsActive;

            var updatedShift = await shiftService.UpdateAsync(shiftToUpdate);
            return Ok(updatedShift);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deactivate(int id)
        {
            // We will add auditing here later if needed
            await shiftService.DeactivateAsync(id);
            return NoContent(); // Standard response for a successful delete
        }
    }
}