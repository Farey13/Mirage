using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
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
            var newItem = new AdminListItem
            {
                ListType = request.ListType,
                ItemValue = request.ItemValue,
                Description = request.Description, // Add this
                IsActive = true
            };
            var createdItem = await adminListService.CreateAsync(newItem);
            return Ok(createdItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAdminListItemRequest request)
        {
            if (id != request.ItemID) return BadRequest("ID mismatch");

            var itemToUpdate = new AdminListItem
            {
                ItemID = request.ItemID,
                ItemValue = request.ItemValue,
                Description = request.Description, // Add this
                IsActive = request.IsActive,
                ListType = ""
            };

            var updatedItem = await adminListService.UpdateAsync(itemToUpdate);
            return Ok(updatedItem);
        }
    }
}