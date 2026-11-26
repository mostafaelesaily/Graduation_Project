using MedLink.Application.Constants;
using MedLink.Application.DTOs.Common.RecommendationDTOs;
using MedLink.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedLink.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.User)]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// Returns recommended providers (internal + external) based on the current
        /// patient's own medical records (Diagnosis + Description).
        /// </summary>
        [HttpPost("[action]")]
        public async Task<IActionResult> GetRecommendations([FromBody] RecommendationRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _recommendationService.GetRecommendationsAsync(CurrentUserId, request);
            return Ok(result);
        }
    }
}
