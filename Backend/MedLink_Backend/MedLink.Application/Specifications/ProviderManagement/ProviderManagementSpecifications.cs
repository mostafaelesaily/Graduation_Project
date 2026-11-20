using MedLink.Domain.Entities.Common;
using System;

namespace MedLink.Application.Specifications.ProviderManagement
{
    public class ProviderProfileByUserSpecification : BaseSpecification<ProviderProfile>
    {
        public ProviderProfileByUserSpecification(string userId)
            : base(p => p.userId == userId)
        {
        }
    }

    public class ProviderLocationsByProviderSpecification : BaseSpecification<ProviderLocations>
    {
        public ProviderLocationsByProviderSpecification(Guid providerProfileId)
            : base(l => l.ProviderProfileId == providerProfileId)
        {
        }
    }

    public class StoredFilesByProviderSpecification : BaseSpecification<StoredFile>
    {
        public StoredFilesByProviderSpecification(Guid providerProfileId)
            : base(f => f.ProviderProfileId == providerProfileId)
        {
        }
    }

    public class ProviderServicesByProviderSpecification : BaseSpecification<ProviderService>
    {
        public ProviderServicesByProviderSpecification(Guid providerProfileId)
            : base(s => s.ProviderProfileId == providerProfileId)
        {
        }
    }

    public class MedicalRecordsByPatientSpecification : BaseSpecification<MedicalRecord>
    {
        public MedicalRecordsByPatientSpecification(string patientId)
            : base(r => r.PatientId == patientId)
        {
        }
    }

    public class MedicalRecordsByProviderSpecification : BaseSpecification<MedicalRecord>
    {
        public MedicalRecordsByProviderSpecification(Guid providerProfileId)
            : base(r => r.ProviderProfileId == providerProfileId)
        {
        }
    }

    public class ReviewsByProviderSpecification : BaseSpecification<Review>
    {
        public ReviewsByProviderSpecification(Guid providerProfileId)
            : base(r => r.ProviderProfileId == providerProfileId)
        {
        }
    }

    public class ReviewsByPatientSpecification : BaseSpecification<Review>
    {
        public ReviewsByPatientSpecification(string patientId)
            : base(r => r.PatientId == patientId)
        {
        }
    }

    public class ProviderAvailabilitiesByProviderSpecification : BaseSpecification<ProviderAvailability>
    {
        public ProviderAvailabilitiesByProviderSpecification(Guid providerProfileId)
            : base(a => a.ProviderProfileId == providerProfileId)
        {
            AddOrderBy(a => a.DayOfWeek);
            AddThenBy(a => a.StartTime);
        }
    }
}