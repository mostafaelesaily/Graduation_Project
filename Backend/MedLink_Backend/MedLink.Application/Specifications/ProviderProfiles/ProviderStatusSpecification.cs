using MedLink.Domain.Entities.Provider;
using MedLink.Domain.Enums;
using System;

namespace MedLink.Application.Specifications.ProviderProfiles
{
    public class ProviderStatusSpecification : BaseSpecification<ProviderProfile>
    {
        public ProviderStatusSpecification(
            int pageNum,
            int pageSize,
            string? search = null,
            VerificationStatus? status = null)
            : base(provider =>
                (status.HasValue
                    ? provider.verificationStatus == status.Value
                    : true) &&
                (string.IsNullOrWhiteSpace(search) ||
                 provider.ProviderServices.Any(service => service.ServiceName.Contains(search)) ||
                 (provider.user != null && provider.user.Email != null && provider.user.Email.Contains(search)) ||
                 (provider.user != null && provider.user.UserName != null && provider.user.UserName.Contains(search))))
        {
            AddInclude(provider => provider.user);
            AddInclude(provider => provider.ProviderServices);

            ApplyPaging((pageNum - 1) * pageSize, pageSize);
        }
    }

    public class ProviderProfileByUserSpecification : BaseSpecification<ProviderProfile>
    {
        public ProviderProfileByUserSpecification(string userId)
            : base(p => p.userId == userId)
        {
        }
    }

    public class ProviderAvailabilitiesByProviderSpecification : BaseSpecification<ProviderAvailability>
    {
        public ProviderAvailabilitiesByProviderSpecification(Guid providerProfileId)
            : base(a => a.ProviderProfileId == providerProfileId)
        {
        }
    }
}