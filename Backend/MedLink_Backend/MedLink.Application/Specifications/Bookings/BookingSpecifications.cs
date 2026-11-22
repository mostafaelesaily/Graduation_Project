using MedLink.Domain.Entities.Common;
using System;

namespace MedLink.Application.Specifications.Bookings
{
    // Shared by both the patient-facing and provider-facing booking services:
    // loading a single booking by id is a neutral data-access concern, not
    // specific to either role.
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
