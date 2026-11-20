using MedLink.Domain.Entities.Common;
using MedLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Specifications.ProviderProfiles
{
    public class ProviderStatusSpecification : BaseSpecification<ProviderProfile> 
    {
        public ProviderStatusSpecification
            (
             int pageNum , 
             int pageSize,
             string? search = null,
             VerificationStatus? status = null
            ) : base(provider =>
             (status.HasValue
                ? provider.verificationStatus == status.Value
                : (provider.verificationStatus == VerificationStatus.Pending ||
                   provider.verificationStatus == VerificationStatus.Rejected)) &&
             (string.IsNullOrWhiteSpace(search) ||
              provider.ServiceName.Contains(search) ||
              (provider.user != null && provider.user.Email != null && provider.user.Email.Contains(search)) ||
              (provider.user != null && provider.user.UserName != null && provider.user.UserName.Contains(search)))
            )
        {
            AddInclude(provider => provider.user);

            ApplyPaging(
            (pageNum - 1) * pageSize,
            pageSize);

        }
    }
}
