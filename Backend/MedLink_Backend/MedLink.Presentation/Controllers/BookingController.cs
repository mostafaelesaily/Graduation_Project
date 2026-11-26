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
    // Patient-facing booking actions. Provider review/approval/cancellation
    // management lives in ProviderController.
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IPatientBookingService bookingService;

        public BookingController(IPatientBookingService bookingService)
        {
            this.bookingService = bookingService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost]
        [Authorize(Roles = Roles.User)]
        public async Task<IActionResult> Book([FromBody] CreateBookingDto dto)
        {
            var result = await bookingService.BookAsync(dto, CurrentUserId);
            return Ok(result);
        }

        [HttpGet("{bookingId:guid}")]
        public async Task<IActionResult> GetMyBookingById([FromRoute] Guid bookingId)
        {
            var result = await bookingService.GetMyBookingByIdAsync(bookingId, CurrentUserId);
            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] int pageNum = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await bookingService.GetMyBookingsAsync(CurrentUserId, pageNum, pageSize);
            return Ok(result);
        }

        [HttpPut("{bookingId:guid}/notes")]
        public async Task<IActionResult> UpdateMyBookingNotes([FromRoute] Guid bookingId, [FromBody] UpdateBookingDto dto)
        {
            await bookingService.UpdateMyBookingNotesAsync(bookingId, dto, CurrentUserId);
            return NoContent();
        }

        [HttpPatch("{bookingId:guid}/cancel")]
        public async Task<IActionResult> CancelMyBooking([FromRoute] Guid bookingId)
        {
            await bookingService.CancelMyBookingAsync(bookingId, CurrentUserId);
            return NoContent();
        }

        [HttpGet("availability")]
        public async Task<IActionResult> CheckAvailability([FromQuery] Guid providerProfileId, [FromQuery] DateTime bookingDate)
        {
            var result = await bookingService.CheckAvailabilityAsync(providerProfileId, bookingDate);
            return Ok(result);
        }
    }
}
