using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/admin/lists")]
    [Authorize(Roles = "Admin")]
    public class AdminListsController(IAdminListService adminListService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await adminListService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("types")]
        public IActionResult GetListTypes()
        {
            // This hard-coded list is the single source of truth for our list types.
            var listTypes = new List<string>
            {
                "MachineName",
                "TestName",
                "KitName",
                "MediaName",
                "RepeatReason",
                "Department"
            };
            return Ok(listTypes.OrderBy(t => t));
        }

        [HttpGet("{listType}")]
        public async Task<IActionResult> GetByType(string listType)
        {
            var items = await adminListService.GetByTypeAsync(listType);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAdminListItemRequest request)
        {
            var actorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!); // Get admin's ID
            var newItem = new AdminListItem
            {
                ListType = request.ListType,
                ItemValue = request.ItemValue,
                Description = request.Description,
                IsActive = true
            };
            var createdItem = await adminListService.CreateAsync(newItem, actorUserId); // Pass the ID
            return Ok(createdItem);
        }


        [HttpGet("setting/{itemValue}")]
        public async Task<IActionResult> GetSetting(string itemValue)
        {
            var setting = await adminListService.GetItemAsync("SystemSetting", itemValue);
            if (setting == null)
            {
                return NotFound();
            }
            return Ok(setting);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAdminListItemRequest request)
        {
            if (id != request.ItemID) return BadRequest("ID mismatch");

            var actorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!); // Get admin's ID
            var itemToUpdate = new AdminListItem
            {
                ItemID = request.ItemID,
                ItemValue = request.ItemValue,
                Description = request.Description,
                IsActive = request.IsActive,
                ListType = "" // ListType is not updated, so it can be empty here
            };

            var updatedItem = await adminListService.UpdateAsync(itemToUpdate, actorUserId); // Pass the ID
            return Ok(updatedItem);
        }
    }
}