using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PortalMirage.Api.Controllers
{
    [ApiController]
    [Route("api/admin/lists")]
    [Authorize(Roles = "Admin")]
    public class AdminListsController(
        IAdminListService adminListService,
        ILogger<AdminListsController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            logger.LogInformation("Fetching all admin list items");
            var items = await adminListService.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("types")]
        public IActionResult GetListTypes()
        {
            logger.LogInformation("Fetching admin list types");
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
            logger.LogInformation("Fetching admin list items for type: {ListType}", listType);
            var items = await adminListService.GetByTypeAsync(listType);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAdminListItemRequest request)
        {
            var actorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            logger.LogInformation("Creating admin list item: {ItemValue} of type {ListType} by user {UserId}", 
                request.ItemValue, request.ListType, actorUserId);
            
            var newItem = new AdminListItem
            {
                ListType = request.ListType,
                ItemValue = request.ItemValue,
                Description = request.Description,
                IsActive = true
            };
            var createdItem = await adminListService.CreateAsync(newItem, actorUserId);
            logger.LogInformation("Admin list item created with ID: {ItemId}", createdItem.ItemID);
            return Ok(createdItem);
        }


        [HttpGet("setting/{itemValue}")]
        public async Task<IActionResult> GetSetting(string itemValue)
        {
            logger.LogInformation("Fetching system setting: {ItemValue}", itemValue);
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

            var actorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            logger.LogInformation("Updating admin list item {ItemId} by user {UserId}", id, actorUserId);
            
            var itemToUpdate = new AdminListItem
            {
                ItemID = request.ItemID,
                ItemValue = request.ItemValue,
                Description = request.Description,
                IsActive = request.IsActive,
                ListType = ""
            };

            var updatedItem = await adminListService.UpdateAsync(itemToUpdate, actorUserId);
            return Ok(updatedItem);
        }
    }
}