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
    public class ProviderController : ControllerBase
    {
        private readonly IProviderManagementService providerService;

        public ProviderController(IProviderManagementService providerService)
        {
            this.providerService = providerService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet("profile")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> GetProfile()
        {
            var result = await providerService.GetMyProfileAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpPut("profile")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProviderProfileDto dto)
        {
            var result = await providerService.UpdateMyProfileAsync(CurrentUserId, dto);
            return Ok(result);
        }

        [HttpGet("locations")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> GetLocations()
        {
            var result = await providerService.GetMyLocationsAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpPost("locations")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> CreateLocation([FromBody] CreateProviderLocationDto dto)
        {
            var result = await providerService.CreateLocationAsync(CurrentUserId, dto);
            return Ok(result);
        }

        [HttpPut("locations/{locationId:guid}")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> UpdateLocation(Guid locationId, [FromBody] UpdateProviderLocationDto dto)
        {
            var result = await providerService.UpdateLocationAsync(CurrentUserId, locationId, dto);
            return Ok(result);
        }

        [HttpDelete("locations/{locationId:guid}")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> DeleteLocation(Guid locationId)
        {
            await providerService.DeleteLocationAsync(CurrentUserId, locationId);
            return NoContent();
        }

        [HttpGet("documents")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> GetDocuments()
        {
            var result = await providerService.GetMyDocumentsAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpPost("documents")]
        [Authorize(Roles = "Provider")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument(Microsoft.AspNetCore.Http.IFormFile file)
        {
            var result = await providerService.UploadDocumentAsync(CurrentUserId, file);
            return Ok(result);
        }

        [HttpDelete("documents/{documentId:guid}")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> DeleteDocument(Guid documentId)
        {
            await providerService.DeleteDocumentAsync(CurrentUserId, documentId);
            return NoContent();
        }

        [HttpGet("services")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> GetServices()
        {
            var result = await providerService.GetMyServicesAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpPost("services")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> CreateService([FromBody] CreateProviderServiceDto dto)
        {
            var result = await providerService.CreateServiceAsync(CurrentUserId, dto);
            return Ok(result);
        }

        [HttpPut("services/{serviceId:guid}")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> UpdateService(Guid serviceId, [FromBody] UpdateProviderServiceDto dto)
        {
            var result = await providerService.UpdateServiceAsync(CurrentUserId, serviceId, dto);
            return Ok(result);
        }

        [HttpDelete("services/{serviceId:guid}")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> DeleteService(Guid serviceId)
        {
            await providerService.DeleteServiceAsync(CurrentUserId, serviceId);
            return NoContent();
        }

        [HttpPost("medical-records")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> CreateMedicalRecord([FromBody] CreateMedicalRecordDto dto)
        {
            var result = await providerService.CreateMedicalRecordAsync(CurrentUserId, dto);
            return Ok(result);
        }

        [HttpGet("medical-records/my")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyMedicalRecords()
        {
            var result = await providerService.GetMyMedicalRecordsAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpGet("medical-records/{recordId:guid}")]
        public async Task<IActionResult> GetMedicalRecord(Guid recordId)
        {
            var result = await providerService.GetMedicalRecordAsync(recordId, CurrentUserId);
            return Ok(result);
        }

        [HttpGet("medical-records/created")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> GetMyCreatedMedicalRecords()
        {
            var result = await providerService.GetMyCreatedMedicalRecordsAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpGet("services/public/{providerProfileId:guid}")]
        public async Task<IActionResult> GetPublicServices(Guid providerProfileId)
        {
            var result = await providerService.GetPublicServicesAsync(providerProfileId);
            return Ok(result);
        }

        [HttpGet("availability")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> GetAvailability()
        {
            var result = await providerService.GetMyAvailabilityAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpPut("availability")]
        [Authorize(Roles = "Provider")]
        public async Task<IActionResult> SetAvailability([FromBody] SetProviderAvailabilityDto dto)
        {
            await providerService.SetMyAvailabilityAsync(CurrentUserId, dto);
            return NoContent();
        }
    }
}