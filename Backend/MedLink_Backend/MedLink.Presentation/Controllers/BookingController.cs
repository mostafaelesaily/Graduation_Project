using MedLink.Application.Constants;
using MedLink.Application.DTOs.Common.BookingDTOs;
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
    public class BookingController : ControllerBase
    {
        private readonly IBookingService bookingService;

        public BookingController(IBookingService bookingService)
        {
            this.bookingService = bookingService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
        {
            var result = await bookingService.CreateBookingAsync(dto, CurrentUserId);
            return Ok(result);
        }

        [HttpPut("{bookingId:guid}/notes")]
        public async Task<IActionResult> UpdateNotes([FromRoute] Guid bookingId, [FromBody] UpdateBookingDto dto)
        {
            await bookingService.UpdateNotesAsync(bookingId, dto, CurrentUserId);
            return NoContent();
        }

        [HttpGet("{bookingId:guid}")]
        public async Task<IActionResult> Get([FromRoute] Guid bookingId)
        {
            var result = await bookingService.GetBookingAsync(bookingId, CurrentUserId);
            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMy(
            [FromQuery] int pageNum = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await bookingService.GetMyBookingsAsync(CurrentUserId, pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("my-provider")]
        [Authorize(Roles = Roles.Provider)]
        public async Task<IActionResult> GetMyProvider(
            [FromQuery] int pageNum = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filter = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? search = null)
        {
            var result = await bookingService.GetMyProviderBookingsAsync(
                CurrentUserId, pageNum, pageSize, filter, startDate, endDate, search);
            return Ok(result);
        }

        [HttpGet("provider/{providerProfileId:guid}")]
        public async Task<IActionResult> GetByProvider(
            [FromRoute] Guid providerProfileId,
            [FromQuery] int pageNum = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filter = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? search = null)
        {
            var result = await bookingService.GetProviderBookingsAsync(
                providerProfileId, CurrentUserId, pageNum, pageSize, filter, startDate, endDate, search);
            return Ok(result);
        }

        [HttpGet("statistics")]
        [Authorize(Roles = Roles.Provider)]
        public async Task<IActionResult> GetStatistics()
        {
            var result = await bookingService.GetProviderBookingStatisticsAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpPatch("{bookingId:guid}/approve")]
        [Authorize(Roles = Roles.Provider)]
        public async Task<IActionResult> Approve([FromRoute] Guid bookingId)
        {
            var result = await bookingService.ApproveBookingAsync(bookingId, CurrentUserId);
            return Ok(result);
        }

        [HttpPatch("{bookingId:guid}/reject")]
        [Authorize(Roles = Roles.Provider)]
        public async Task<IActionResult> Reject([FromRoute] Guid bookingId)
        {
            await bookingService.RejectBookingAsync(bookingId, CurrentUserId);
            return NoContent();
        }

        [HttpPatch("{bookingId:guid}/cancel")]
        public async Task<IActionResult> Cancel([FromRoute] Guid bookingId)
        {
            await bookingService.CancelBookingAsync(bookingId, CurrentUserId);
            return NoContent();
        }

        [HttpGet("availability")]
        public async Task<IActionResult> Availability([FromQuery] Guid providerProfileId, [FromQuery] DateTime bookingDate)
        {
            var result = await bookingService.CheckAvailabilityAsync(providerProfileId, bookingDate);
            return Ok(result);
        }
    }
}