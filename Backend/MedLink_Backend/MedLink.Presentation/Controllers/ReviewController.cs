using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MedLink.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IProviderManagementService reviewService;

        public ReviewController(IProviderManagementService reviewService)
        {
            this.reviewService = reviewService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
        {
            var result = await reviewService.CreateReviewAsync(CurrentUserId, dto);
            return Ok(result);
        }

        [HttpGet("provider/{providerProfileId:guid}")]
        public async Task<IActionResult> GetByProvider(Guid providerProfileId)
        {
            var result = await reviewService.GetProviderReviewsAsync(providerProfileId);
            return Ok(result);
        }

        [HttpGet("my")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMy()
        {
            var result = await reviewService.GetMyReviewsAsync(CurrentUserId);
            return Ok(result);
        }
    }
}