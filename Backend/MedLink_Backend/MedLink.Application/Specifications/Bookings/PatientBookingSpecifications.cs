using MedLink.Domain.Entities.Common;
using MedLink.Domain.Enums;
using System;

namespace MedLink.Application.Specifications.Bookings
{
    // Used by the patient booking flow (creating a booking, checking availability)
    // and by the background expiration process.
    public class BookingConflictSpecification : BaseSpecification<Booking>
    {
        public BookingConflictSpecification(Guid providerProfileId, DateTime bookingDate, DateTime? asOfUtc = null)
            : base(b => b.ProviderProfileId == providerProfileId
                        && b.BookingDate == bookingDate
                        && b.Status != BookingStatus.Cancelled
                        && b.Status != BookingStatus.Rejected
                        && b.Status != BookingStatus.Expired
                        && !(b.Status == BookingStatus.Pending
                             && b.CreatedAt <= (asOfUtc ?? DateTime.UtcNow).AddHours(-8)))
        {
        }
    }

    public class OverduePendingBookingsSpecification : BaseSpecification<Booking>
    {
        public OverduePendingBookingsSpecification(
            Guid? providerProfileId = null,
            DateTime? bookingDate = null,
            DateTime? asOfUtc = null)
            : base(b => b.Status == BookingStatus.Pending
                        && (!providerProfileId.HasValue || b.ProviderProfileId == providerProfileId.Value)
                        && (!bookingDate.HasValue || b.BookingDate == bookingDate.Value)
                        && b.CreatedAt <= (asOfUtc ?? DateTime.UtcNow).AddHours(-8))
        {
        }
    }

    public class BookingsByUserSpecification : BaseSpecification<Booking>
    {
        public BookingsByUserSpecification(string userId)
            : base(b => b.UserId == userId)
        {
            AddInclude(b => b.User);
            AddOrderByDescending(b => b.CreatedAt);
        }

        public BookingsByUserSpecification(string userId, int pageNumber, int pageSize)
            : base(b => b.UserId == userId)
        {
            AddInclude(b => b.User);
            AddOrderByDescending(b => b.CreatedAt);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}
