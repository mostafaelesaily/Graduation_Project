using MedLink.Domain.Entities.Common;
using MedLink.Domain.Enums;
using System;

namespace MedLink.Application.Specifications.Bookings
{
    // Used by the provider booking-management flow: listing/searching a
    // provider's own bookings, bulk operations, and dashboard statistics.
    public class ProviderTodayBookingsSpecification : BaseSpecification<Booking>
    {
        public ProviderTodayBookingsSpecification(Guid providerProfileId, DateTime dayStart)
            : base(b => b.ProviderProfileId == providerProfileId
                        && b.BookingDate >= dayStart
                        && b.BookingDate < dayStart.AddDays(1))
        {
        }
    }

    public class ProviderNextSevenDaysBookingsSpecification : BaseSpecification<Booking>
    {
        public ProviderNextSevenDaysBookingsSpecification(Guid providerProfileId, DateTime start, DateTime endExclusive)
            : base(b => b.ProviderProfileId == providerProfileId
                        && b.BookingDate >= start
                        && b.BookingDate < endExclusive
                        && b.Status != BookingStatus.Cancelled
                        && b.Status != BookingStatus.Expired)
        {
        }
    }

    public class ProviderPendingBookingsSpecification : BaseSpecification<Booking>
    {
        public ProviderPendingBookingsSpecification(Guid providerProfileId)
            : base(b => b.ProviderProfileId == providerProfileId
                        && b.Status == BookingStatus.Pending)
        {
        }
    }

    public class ProviderCancelledBookingsSpecification : BaseSpecification<Booking>
    {
        public ProviderCancelledBookingsSpecification(Guid providerProfileId)
            : base(b => b.ProviderProfileId == providerProfileId
                        && b.Status == BookingStatus.Cancelled)
        {
        }
    }

    public class BookingsByProviderSpecification : BaseSpecification<Booking>
    {
        public BookingsByProviderSpecification(
            Guid providerProfileId,
            string? filter = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? search = null)
            : base(b => b.ProviderProfileId == providerProfileId
                        && (string.IsNullOrWhiteSpace(filter)
                            || filter == "upcoming" && b.BookingDate >= DateTime.UtcNow && b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Expired && b.Status != BookingStatus.Rejected
                            || filter == "today" && b.BookingDate >= DateTime.UtcNow.Date && b.BookingDate < DateTime.UtcNow.Date.AddDays(1)
                            || filter == "past" && b.BookingDate < DateTime.UtcNow
                            || filter == "cancelled" && b.Status == BookingStatus.Cancelled
                            || filter == "next7days" && b.BookingDate >= DateTime.UtcNow && b.BookingDate < DateTime.UtcNow.AddDays(7) && b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Expired && b.Status != BookingStatus.Rejected)
                        && (!startDate.HasValue || b.BookingDate >= startDate.Value)
                        && (!endDate.HasValue || b.BookingDate <= endDate.Value)
                        && (string.IsNullOrWhiteSpace(search)
                            || (b.User != null && b.User.UserName != null && b.User.UserName.Contains(search))
                            || (b.User != null && b.User.Email != null && b.User.Email.Contains(search))
                            || (b.Notes != null && b.Notes.Contains(search))))
        {
            AddInclude(b => b.User);
            AddInclude(b => b.ProviderProfile);
            AddOrderByDescending(b => b.CreatedAt);
        }

        public BookingsByProviderSpecification(
            Guid providerProfileId,
            int pageNumber,
            int pageSize,
            string? filter = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? search = null)
            : base(b => b.ProviderProfileId == providerProfileId
                        && (string.IsNullOrWhiteSpace(filter)
                            || filter == "upcoming" && b.BookingDate >= DateTime.UtcNow && b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Expired && b.Status != BookingStatus.Rejected
                            || filter == "today" && b.BookingDate >= DateTime.UtcNow.Date && b.BookingDate < DateTime.UtcNow.Date.AddDays(1)
                            || filter == "past" && b.BookingDate < DateTime.UtcNow
                            || filter == "cancelled" && b.Status == BookingStatus.Cancelled
                            || filter == "next7days" && b.BookingDate >= DateTime.UtcNow && b.BookingDate < DateTime.UtcNow.AddDays(7) && b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Expired && b.Status != BookingStatus.Rejected)
                        && (!startDate.HasValue || b.BookingDate >= startDate.Value)
                        && (!endDate.HasValue || b.BookingDate <= endDate.Value)
                        && (string.IsNullOrWhiteSpace(search)
                            || (b.User != null && b.User.UserName != null && b.User.UserName.Contains(search))
                            || (b.User != null && b.User.Email != null && b.User.Email.Contains(search))
                            || (b.Notes != null && b.Notes.Contains(search))))
        {
            AddInclude(b => b.User);
            AddInclude(b => b.ProviderProfile);
            AddOrderByDescending(b => b.CreatedAt);
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}
