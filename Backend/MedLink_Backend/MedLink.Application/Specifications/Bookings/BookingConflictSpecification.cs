using MedLink.Domain.Entities.Common;
using MedLink.Domain.Enums;
using System;

namespace MedLink.Application.Specifications.Bookings
{
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
        public OverduePendingBookingsSpecification(DateTime? asOfUtc = null)
            : base(b => b.Status == BookingStatus.Pending
                        && b.CreatedAt <= (asOfUtc ?? DateTime.UtcNow).AddHours(-8))
        {
        }
    }

    public class BookingsByUserSpecification : BaseSpecification<Booking>
    {
        public BookingsByUserSpecification(string userId)
            : base(b => b.UserId == userId)
        {
            AddOrderByDescending(b => b.CreatedAt);
        }

        public BookingsByUserSpecification(string userId, int pageNumber, int pageSize)
            : base(b => b.UserId == userId)
        {
            AddOrderByDescending(b => b.CreatedAt);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }

    public class BookingsByProviderSpecification : BaseSpecification<Booking>
    {
        public BookingsByProviderSpecification(Guid providerProfileId)
            : base(b => b.ProviderProfileId == providerProfileId)
        {
            AddOrderByDescending(b => b.CreatedAt);
        }

        public BookingsByProviderSpecification(Guid providerProfileId, int pageNumber, int pageSize)
            : base(b => b.ProviderProfileId == providerProfileId)
        {
            AddOrderByDescending(b => b.CreatedAt);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }

    public class BookingByIdSpecification : BaseSpecification<Booking>
    {
        public BookingByIdSpecification(Guid bookingId)
            : base(b => b.Id == bookingId)
        {
            AddInclude(b => b.User);
            AddInclude(b => b.ProviderProfile);
        }
    }
}
