using MedLink.Application.DTOs.Common.MedicalSearchDTOs;
using MedLink.Application.Interfaces.Common;
using Microsoft.AspNetCore.Mvc;

namespace MedLink.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IMedicalSearchService _medicalSearchService;

        public SearchController(IMedicalSearchService medicalSearchService)
        {
            _medicalSearchService = medicalSearchService;
        }

        /// <summary>
        /// Performs unified medical search - searches both internal MedLink providers and external providers
        /// </summary>
        /// <param name="request">Medical search request with keyword, location, and radius</param>
        /// <returns>Unified search results containing both internal and external providers</returns>
        [HttpPost("[action]")]
        public async Task<IActionResult> UnifiedMedicalSearch([FromBody] MedicalSearchRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _medicalSearchService.SearchAsync(request);
            return Ok(result);
        }
    }
}
