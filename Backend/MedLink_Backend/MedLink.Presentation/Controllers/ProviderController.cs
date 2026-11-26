using MedLink.Application.Constants;
using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MedLink.Presentation.Controllers
{
    // Provider-management actions: availability configuration and
    // booking review/approval/rejection for the provider's own profile.
    // Cancelling a booking is a patient-only action (see BookingController).
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProviderController : ControllerBase
    {
        private readonly IProviderAvailabilityService availabilityService;
        private readonly IProviderBookingService bookingService;

        public ProviderController(
            IProviderAvailabilityService availabilityService,
            IProviderBookingService bookingService)
        {
            this.availabilityService = availabilityService;
            this.bookingService = bookingService;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ----- Availability management -----

        [HttpGet("availability")]
        [Authorize(Roles = Roles.Provider)]
        public async Task<IActionResult> GetMyAvailability()
        {
            var result = await availabilityService.GetMyAvailabilityAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpPut("availability")]
        [Authorize(Roles = Roles.Provider)]
        public async Task<IActionResult> UpdateMyAvailability([FromBody] EditAvailabilityDto dto)
        {
            await availabilityService.SetMyAvailabilityAsync(CurrentUserId, dto);
            return NoContent();
        }

        // ----- Booking management -----

        [HttpGet("bookings/my")]
        [Authorize(Roles = Roles.Provider)]
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] int pageNum = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filter = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? search = null)
        {
            var result = await bookingService.GetMyBookingsAsync(
                CurrentUserId, pageNum, pageSize, filter, startDate, endDate, search);
            return Ok(result);
        }

        [HttpGet("{providerProfileId:guid}/bookings")]
        public async Task<IActionResult> GetBookingsByProvider(
            [FromRoute] Guid providerProfileId,
            [FromQuery] int pageNum = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filter = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? search = null)
        {
            var result = await bookingService.GetBookingsByProviderIdAsync(
                providerProfileId, CurrentUserId, pageNum, pageSize, filter, startDate, endDate, search);
            return Ok(result);
        }

        [HttpGet("bookings/{bookingId:guid}")]
        [Authorize(Roles = Roles.Provider)]
        public async Task<IActionResult> GetBookingById([FromRoute] Guid bookingId)
        {
            var result = await bookingService.GetBookingByIdAsync(bookingId, CurrentUserId);
            return Ok(result);
        }

        [HttpGet("bookings/statistics")]
        [Authorize(Roles = Roles.Provider)]
        public async Task<IActionResult> GetBookingStatistics()
        {
            var result = await bookingService.GetBookingStatisticsAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpPatch("bookings/{bookingId:guid}/approve")]
        [Authorize(Roles = Roles.Provider)]
        public async Task<IActionResult> ApproveBooking([FromRoute] Guid bookingId)
        {
            var result = await bookingService.ApproveBookingAsync(bookingId, CurrentUserId);
            return Ok(result);
        }

        [HttpPatch("bookings/{bookingId:guid}/reject")]
        [Authorize(Roles = Roles.Provider)]
        public async Task<IActionResult> RejectBooking([FromRoute] Guid bookingId)
        {
            await bookingService.RejectBookingAsync(bookingId, CurrentUserId);
            return NoContent();
        }
    }
}
