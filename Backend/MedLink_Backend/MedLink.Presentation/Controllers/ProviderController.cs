using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MedLink.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderController : ControllerBase
    {
        private readonly IProviderAvailabilityService availabilityService;

        public ProviderController(IProviderAvailabilityService availabilityService)
        {
            this.availabilityService = availabilityService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet("availability")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> GetAvailability()
        {
            var result = await availabilityService.GetMyAvailabilityAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpPut("availability")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> SetAvailability([FromBody] EditAvailabilityDto dto)
        {
            await availabilityService.SetMyAvailabilityAsync(CurrentUserId, dto);
            return NoContent();
        }
    }
}